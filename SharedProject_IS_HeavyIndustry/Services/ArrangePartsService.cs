using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using DynamicData;
using SharedProject_IS_HeavyIndustry.Models;
using Google.OrTools.LinearSolver;
using Google.OrTools.Sat;
using Newtonsoft.Json.Linq;
using SharedProject_IS_HeavyIndustry.Converters;
using SharedProject_IS_HeavyIndustry.ViewModels;
using Constraint = Google.OrTools.LinearSolver.Constraint;

namespace SharedProject_IS_HeavyIndustry.Services;

public class ArrangePartsService
{
    public static List<int> _lengthOptionsRawMaterial { get; set; }
    private static ObservableCollection<RawMaterial> _rawMaterialsUsed;
    private static List<Part> _separatedParts;

    // Constructor
    public ArrangePartsService(List<Part> parts, ObservableCollection<Part> overSizeParts, List<int> lengthOptions)
    {
        foreach (var part in overSizeParts)
        {
            Console.WriteLine("분리길이 : " + part.lengthToBeSeparated);
        }
        
        // 파트배치 완료된 것들
        _lengthOptionsRawMaterial = lengthOptions;

        List<Part> replacedParts = new List<Part>();
        
        // 파트배치 하기 전에 분리하는 작업
        foreach (var ppp in overSizeParts)
        {
            int length = ppp.Length;
            if (!string.IsNullOrEmpty(ppp.lengthToBeSeparated))
            {
                int lengthToBeSeperated = Convert.ToInt32(ppp.lengthToBeSeparated);
                while (length > lengthToBeSeperated)
                {
                    length -= lengthToBeSeperated;
                    // 분리한 놈들은 J를 붙여서 새로운 피트로 추가
                    Part newPart = new Part(ppp.Assem, ppp.Mark.Insert(0, "J_"), ppp.Material,
                        lengthToBeSeperated, ppp.Num, ppp.WeightOne, ppp.WeightSum, ppp.PArea,
                        ppp.Desc);
                    replacedParts.Add(newPart);
                }
                
                if (length > 0)
                {
                    // 여기에서 블록마크에 J 붙이는 코드 추가해야함.
                    Part restPart = new Part(ppp.Assem, ppp.Mark.Insert(0, "J_"), ppp.Material, length, ppp.Num,
                        ppp.WeightOne, ppp.WeightSum, ppp.PArea, ppp.Desc);
                    replacedParts.Add(restPart);
                }
            }
        }

        _separatedParts = replacedParts;
        foreach (var VARIABLE in _separatedParts)
        {
            Console.WriteLine(VARIABLE);
        }
        parts.AddRange(_separatedParts);
        _rawMaterialsUsed = ArrangeParts(parts);

    }

    public static ObservableCollection<RawMaterial> ArrangeParts(List<Part> parts)
    {
        List<RawMaterial> rawMaterialsUsed = new List<RawMaterial>();
        // List<Part> partList = ExcelDataLoader.PartListFromExcel("/Users/suchacoolguy/Documents/BOM_test.xlsx");
        List<Part> partList = parts;
        // sort in descending order
        partList.Sort((a, b) => b.Length.CompareTo(a.Length));
        // sort in descending order
        _lengthOptionsRawMaterial.Sort((a, b) => b.CompareTo(a));


        int totalPartsLength = GetTotalPartsLength(partList);
        int avgPartsLength = totalPartsLength / partList.Count;
        int partCnt = partList.Count;
        int medianLength = partList[partCnt / 2].Length;
        
        Console.WriteLine("Average Parts Length: " + avgPartsLength);
        Console.WriteLine("Median Length: " + medianLength);
        
        // Create the linear solver with the SCIP backend.
        Solver solver = Solver.CreateSolver("SCIP");
        

        DataModel data;
        
        if (partCnt > 200)
        {
            if (avgPartsLength < _lengthOptionsRawMaterial[0] * 0.2 &&
                medianLength < _lengthOptionsRawMaterial[0] * 0.2)
            {
                Console.WriteLine("2차원 배열 사이즈 팍 줄인다 실시!");
                data = new DataModel(partList, _lengthOptionsRawMaterial, Convert.ToInt32(partCnt / 8 + 10));   
            }
            else 
            {
                data = new DataModel(partList, _lengthOptionsRawMaterial);
            }
        }
        else
        {
            data = new DataModel(partList, _lengthOptionsRawMaterial);
        }
            
            
        
        // create 2d array of variables. x[i, j] is 1 if item i is in bin j.
        Variable[,] x = new Variable[data.NumItems, data.NumBins];
        for (int i = 0; i < data.NumItems; i++)
        {
            for (int j = 0; j < data.NumBins; j++)
            {
                // x[i, j] is 1 if item i is packed in bin j. otherwise 0.
                x[i, j] = solver.MakeIntVar(0, 1, $"x_{i}_{j}");
            }
        }
        // row i represents the i-th bin and column j represents the length of the bin.
        Variable[,] y = new Variable[data.NumBins, data.NumRawMaterialOptions];
        for (int i = 0; i < data.NumBins; i++)
        {
            for (int j = 0; j < data.NumRawMaterialOptions; j++)
            {
                // y[i, j] is 1 if bin i has length j. otherwise 0.
                y[i, j] = solver.MakeIntVar(0, 1, $"y_{i}_{j}");
            }    
        }
        
        for (int i = 0; i < data.NumItems; i++)
        {
            // each item is in exactly one bin. every item must be in one bin.
            Constraint constraint = solver.MakeConstraint(1, 1, "");
            for (int j = 0; j < data.NumBins; j++)
            {
                constraint.SetCoefficient(x[i, j], 1);
            }
        }

        for (int i = 0; i < data.NumBins; i++)
        {
            // each bin has exactly one length. every bin must have one length.
            Constraint constraint = solver.MakeConstraint(0, 1, "");
            for (int j = 0; j < data.NumRawMaterialOptions; j++)
            {
                constraint.SetCoefficient(y[i, j], 1);
            }
        }

        // the sum of the lengths of the items in each bin must be less than or equal to the bin's capacity.
        for (int j = 0; j < data.NumBins; j++)
        {
            Constraint constraint = solver.MakeConstraint(0, Double.PositiveInfinity, "");
            
            // get the length of the bin.
            for (int i = 0; i < data.NumRawMaterialOptions; i++)
            {
                constraint.SetCoefficient(y[j, i], DataModel.lengthOptionsRawMaterial[i]);
            }
            
            // BinCapacity - (Sum of the lengths of the items in the bin) >= 0
            // since we set the lower bound to 0, the sum of the lengths of the items in the bin must be less than or equal to the bin's capacity.
            for (int i = 0; i < data.NumItems; i++)
            {
                constraint.SetCoefficient(x[i, j], -DataModel.parts[i].Length);
            }
        }

        Objective objective = solver.Objective();
        // set the objective to minimize the total sum of the remaining lengths of the bins.
        // 목적함수를 설정해두면 이걸 최소화하도록 하는 메소드를 사용하는 것임.
        for (int j = 0; j < data.NumBins; j++)
        {
            for (int k = 0; k < data.NumRawMaterialOptions; k++)
            {
                // Objective: minimize the total waste
                objective.SetCoefficient(y[j, k], DataModel.lengthOptionsRawMaterial[k]);
            }
            for (int i = 0; i < data.NumItems; i++)
            {
                // Subtract the parts lengths from the total raw material length
                objective.SetCoefficient(x[i, j], -DataModel.parts[i].Length);
            }
        }
        
        objective.SetMinimization();

        Console.WriteLine("Ready to solve.");

        solver.SetSolverSpecificParametersAsString("limits/solutions = 50");
        solver.SetTimeLimit(15000);
        
        var watch = System.Diagnostics.Stopwatch.StartNew();
        Solver.ResultStatus resultStatus = solver.Solve();
        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;
        Console.WriteLine("Elapsed Time: " + elapsedMs);

        Console.WriteLine("Solved.");
        
        Console.WriteLine($"Total Scrap: {solver.Objective().Value()}");
        
        if (resultStatus == Solver.ResultStatus.NOT_SOLVED || resultStatus == Solver.ResultStatus.ABNORMAL)
        {
            Console.WriteLine("파트 배치에 실패하였습니다. 다시 시도해주세요.");
            return new ObservableCollection<RawMaterial>();
        }
        else if (resultStatus == Solver.ResultStatus.INFEASIBLE)
        {
            // 파트가 없거나 하나밖에 없을 때 솔루션을 찾지 못하는 경우 여기로 들어옴.
            MessageService.Send("파트 배치가 불가능합니다. 규격 설정과 분리 설정을 확인해주세요.");
            return new ObservableCollection<RawMaterial>();
        }
        else
        {
            int howManyTimes = 0;
            int TotalScrap = 0;
            int BinLength = 0;
        
            bool foundBin = false;
            for (int j = 0; j < data.NumBins; ++j)
            {
                RawMaterial rawMaterial = null;
                for (int i = 0; i < data.NumRawMaterialOptions; i++)
                {
                    if (y[j, i].SolutionValue() == 1)
                    {
                    
                        BinLength = DataModel.lengthOptionsRawMaterial[i];
                        rawMaterial = new RawMaterial(BinLength);
                    }
                }

                for (int i = 0; i < data.NumItems; i++)
                {
                    if (x[i, j].SolutionValue() == 1)
                    {
                        howManyTimes++;
                        rawMaterial.insert_part(DataModel.parts[i]);
                    }
                }
                if (rawMaterial != null)
                    rawMaterialsUsed.Add(rawMaterial);
            }
            Console.WriteLine("How Many Times: " + howManyTimes);
            rawMaterialsUsed.Sort((x, y) => x.Length.CompareTo(y.Length));
            ObservableCollection<RawMaterial> res = new ObservableCollection<RawMaterial>(rawMaterialsUsed); 
            return res;
        }
    }

private static int GetTotalPartsLength(List<Part> partList)
    {
        int totalPartsLength = 0;
        foreach (var part in partList)
        {
            totalPartsLength += part.Length;
        }

        return totalPartsLength;
    }
    
    private static int FindLongestPartLength(List<Part> partList)
    {
        int longestLength = 0;
        foreach (var part in partList)
        {
            if (longestLength < part.Length)
                longestLength = part.Length;
        }

        if (longestLength > 0)
            return longestLength;
        else
            return -1;
    }

    public ObservableCollection<RawMaterial> GetArrangedRawMaterials()
    {
        return _rawMaterialsUsed;
    }
    
    public List<Part> GetOverSizeParts()
    {
        return _separatedParts;
    }
    
    public static List<int> GetLengthOptionsRawMaterial()
    {
        return _lengthOptionsRawMaterial;
    }

    private static void count_check(List<RawMaterial> rawMaterialUsed)
    {
        int count = 0;
        foreach (var raw in rawMaterialUsed)
        {
            foreach (var part in raw.PartsInside)
            {
                count++;
            }
        }
        
        Console.WriteLine("Total parts after 배치: " + count);
        Console.WriteLine("Total raw materials after 배치: " + rawMaterialUsed.Count);
    }
}

public class VarArraySolutionPrinterWithLimit : CpSolverSolutionCallback
{
    
    private int solution_count_;
    private IntVar[][] variables_;
    private int solution_limit_;
    
    public VarArraySolutionPrinterWithLimit(int solution_limit)
    {
        solution_limit_ = solution_limit;
    }

    public override void OnSolutionCallback()
    {
        Console.WriteLine(String.Format("Solution #{0}: time = {1:F2} s", solution_count_, WallTime()));
        solution_count_++;
        if (solution_count_ >= solution_limit_)
        {
            Console.WriteLine(String.Format("Stopping search after {0} solutions", solution_limit_));
            StopSearch();
        }
    }

    public int SolutionCount()
    {
        return solution_count_;
    }
}
