using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using DynamicData;
using SharedProject_IS_HeavyIndustry.Models;
using Google.OrTools.LinearSolver;
using Newtonsoft.Json.Linq;
using SharedProject_IS_HeavyIndustry.Converters;

namespace SharedProject_IS_HeavyIndustry.Services;

public class ArrangePartsService
{
    public static List<int> _lengthOptionsRawMaterial { get; set; }
    private static ObservableCollection<RawMaterial> _rawMaterialsUsed;
    private static List<Part> _separatedParts;

    // Constructor
    public ArrangePartsService(List<Part> parts, ObservableCollection<Part> overSizeParts, List<int> lengthOptions)
    {
        // 파트배치 완료된 것들
        _lengthOptionsRawMaterial = lengthOptions;

        List<Part> replacedParts = new List<Part>();
        foreach (var ppp in overSizeParts)
        {
            int length = ppp.Length;
            while (length > Convert.ToInt32(ppp.lengthToBeSeperated))
            {
                length -= Convert.ToInt32(ppp.lengthToBeSeperated);
                // 여기에서 블록마크에 J 붙이는 코드 추가해야함.
                Part newPart = new Part(ppp.Assem, ppp.Mark.Insert(0, "J_"), ppp.Material,
                    Convert.ToInt32(ppp.lengthToBeSeperated), ppp.Num, ppp.WeightOne, ppp.WeightSum, ppp.PArea,
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

        _separatedParts = replacedParts;
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

        int selectedRawMaterialLength = 0;
        int totalPartsLength = GetTotalPartsLength(partList);
        foreach (var len in _lengthOptionsRawMaterial)
        {
            if (totalPartsLength <= len)
            {
                selectedRawMaterialLength = len;
            }
        }

        int longestPartLength = -1;
        if (selectedRawMaterialLength == 0)
        {
            longestPartLength = FindLongestPartLength(partList);
            if (longestPartLength != -1)
            {
                foreach (var rawLength in _lengthOptionsRawMaterial)
                {
                    if (rawLength >= longestPartLength)
                        selectedRawMaterialLength = rawLength;
                }    
            }
        }
        
        if (selectedRawMaterialLength == 0)
        {
            MessageService.Send("배치 가능한 원자재 길이가 없습니다. 규격 설정 및 분리길이 설정을 확인해주세요.");
            return new ObservableCollection<RawMaterial>();
        }
        else
        {
            DataModel data;
            
            int avgPartsLength = totalPartsLength / partList.Count;
            Console.WriteLine("Avg Parts Length: " + avgPartsLength);
            Console.WriteLine("Median: " + partList[partList.Count / 2].Length);
            Console.WriteLine("Selected Raw Material Length: " + selectedRawMaterialLength);
            if (longestPartLength != -1)
            {
                if (avgPartsLength < selectedRawMaterialLength * 0.1)
                {
                    Console.WriteLine("옳거니! 제곱근");
                    data = new DataModel(partList, _lengthOptionsRawMaterial, Convert.ToInt32(Math.Sqrt(selectedRawMaterialLength)));
                }
                else if (partList[partList.Count / 2].Length < selectedRawMaterialLength * 0.1 && partList.Count > 200)
                {
                    Console.WriteLine("파트는 많지만 제곱근으로 줄이면 배치 불가넝 (ㅇ..ㅇ;;) ");
                    data = new DataModel(partList, _lengthOptionsRawMaterial, partList.Count / 4);
                }
                else if (avgPartsLength < selectedRawMaterialLength / 2 && partList[partList.Count / 2].Length < selectedRawMaterialLength / 2)
                {
                    Console.WriteLine("옳거니! 2분의 1");
                    data = new DataModel(partList, _lengthOptionsRawMaterial, partList.Count / 2 + 2);
                }
                else if (partList[partList.Count / 2].Length > selectedRawMaterialLength / 2)
                {
                    if (avgPartsLength > selectedRawMaterialLength * 0.75 &&
                        partList[partList.Count / 2].Length > selectedRawMaterialLength * 0.75)
                    {
                        Console.WriteLine("작은 원자재 길이 쓰면 딱 맞을듯^^");
                        data = new DataModel(partList, _lengthOptionsRawMaterial, partList.Count);
                    }
                    else
                    {
                        Console.WriteLine("중간값이 2분의 1보다 큼");
                        selectedRawMaterialLength = _lengthOptionsRawMaterial[0];
                        data = new DataModel(partList, _lengthOptionsRawMaterial, partList.Count);    
                    }
                    
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
            
            
            Solver solver = Solver.CreateSolver("CP-SAT");
            solver.SetNumThreads(8);
            solver.SetTimeLimit(15000);
        
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
            Variable[] y = new Variable[data.NumBins];
            for (int i = 0; i < data.NumBins; i++)
            {
                    // y[i, j] is 1 if bin i has length j. otherwise 0.
                y[i] = solver.MakeIntVar(0, 1, $"y_{i}");
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

            // the sum of the lengths of the items in each bin must be less than or equal to the bin's capacity.
            for (int j = 0; j < data.NumBins; j++)
            {
                Constraint constraint = solver.MakeConstraint(0, Double.PositiveInfinity, "");
                constraint.SetCoefficient(y[j], selectedRawMaterialLength);
                
                // BinCapacity - (Sum of the lengths of the items in the bin) >= 0
                // since we set the lower bound to 0, the sum of the lengths of the items in the bin must be less than or equal to the bin's capacity.
                for (int i = 0; i < data.NumItems; i++)
                {
                    constraint.SetCoefficient(x[i, j], -DataModel.parts[i].Length);
                }
            }

            Objective objective = solver.Objective();
            // set the objective to minimize the total sum of the remaining lengths of the bins.
        
            for (int j = 0; j < data.NumBins; j++)
            {
                objective.SetCoefficient(y[j], selectedRawMaterialLength);
            }
        
            for (int j = 0; j < data.NumBins; j++)
            {
                for (int i = 0; i < data.NumItems; i++)
                {
                    // Subtract the parts lengths from the total raw material length
                    objective.SetCoefficient(x[i, j], -DataModel.parts[i].Length);
                }
            }

            objective.SetMinimization();

            Console.WriteLine("Ready to solve.");

            Solver.ResultStatus resultStatus = solver.Solve();

            Console.WriteLine("Solved.");
            // Check that the problem has an optimal solution.
            
            if (resultStatus == Solver.ResultStatus.INFEASIBLE)
            {
                // 파트가 없거나 하나밖에 없을 때 솔루션을 찾지 못하는 경우 여기로 들어옴.
                MessageService.Send("파트 배치가 불가능합니다. 규격 설정과 분리 설정을 확인해주세요.");
                return new ObservableCollection<RawMaterial>();
            }
            else if (resultStatus == Solver.ResultStatus.NOT_SOLVED || resultStatus == Solver.ResultStatus.ABNORMAL)
            {
                Console.WriteLine("파트 배치에 실패하였습니다. 다시 시도해주세요.");
                return new ObservableCollection<RawMaterial>();
            }
            else
            {
                Console.WriteLine($"Total Scrap: {solver.Objective().Value()}");

                int howManyTimes = 0;
                int TotalScrap = 0;
        
                bool foundBin = false;
                for (int j = 0; j < data.NumBins; ++j)
                {
                    RawMaterial rawMaterial = null;
                    if (y[j].SolutionValue() == 1)
                    {
                        rawMaterial = new RawMaterial(selectedRawMaterialLength);
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
                count_check(rawMaterialsUsed);
                rawMaterialsUsed.Sort((x, y) => x.Length.CompareTo(y.Length));
                ObservableCollection<RawMaterial> res = new ObservableCollection<RawMaterial>(rawMaterialsUsed); 
                return res;
            }
        
        }

        return new ObservableCollection<RawMaterial>();
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