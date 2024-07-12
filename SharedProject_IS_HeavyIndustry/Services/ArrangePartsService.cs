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

            DataModel data = new DataModel(partList, _lengthOptionsRawMaterial);

            foreach (var ppp in partList)
            {
                Console.WriteLine(ppp.Length);
            }

            // Create the linear solver with the SCIP backend.
            Solver solver = Solver.CreateSolver("SCIP");
            solver.SetTimeLimit(7000);

            // Create 2D array of variables. x[i, j] is 1 if item i is in bin j.
            Variable[,] x = new Variable[data.NumItems, data.NumBins];
            for (int i = 0; i < data.NumItems; i++)
            {
                for (int j = 0; j < data.NumBins; j++)
                {
                    x[i, j] = solver.MakeIntVar(0, 1, $"x_{i}_{j}");
                }
            }

            // Create 2D array of variables. y[i, j] is 1 if bin i has length j.
            Variable[,] y = new Variable[data.NumBins, data.NumRawMaterialOptions];
            for (int i = 0; i < data.NumBins; i++)
            {
                for (int j = 0; j < data.NumRawMaterialOptions; j++)
                {
                    y[i, j] = solver.MakeIntVar(0, 1, $"y_{i}_{j}");
                }
            }

            for (int i = 0; i < data.NumItems; i++)
            {
                // Each item is in exactly one bin. Every item must be in one bin.
                Constraint constraint = solver.MakeConstraint(1, 1, "");
                for (int j = 0; j < data.NumBins; j++)
                {
                    constraint.SetCoefficient(x[i, j], 1);
                }
            }

            for (int i = 0; i < data.NumBins; i++)
            {
                // Each bin has exactly one length. Every bin must have one length.
                Constraint constraint = solver.MakeConstraint(0, 1, "");
                for (int j = 0; j < data.NumRawMaterialOptions; j++)
                {
                    constraint.SetCoefficient(y[i, j], 1);
                }
            }

            // The sum of the lengths of the items in each bin must be less than or equal to the bin's capacity.
            for (int j = 0; j < data.NumBins; j++)
            {
                Constraint constraint = solver.MakeConstraint(0, Double.PositiveInfinity, "");

                // Get the length of the bin.
                for (int i = 0; i < data.NumRawMaterialOptions; i++)
                {
                    constraint.SetCoefficient(y[j, i], DataModel.lengthOptionsRawMaterial[i]);
                }

                // BinCapacity - (Sum of the lengths of the items in the bin) >= 0
                // Since we set the lower bound to 0, the sum of the lengths of the items in the bin must be less than or equal to the bin's capacity.
                for (int i = 0; i < data.NumItems; i++)
                {
                    constraint.SetCoefficient(x[i, j], -DataModel.parts[i].Length);
                }
            }

            // Objective: Minimize the total waste + penalty for using different sizes
            Objective objective = solver.Objective();
            int penalty = 1000; // Adjust the penalty value as needed

            for (int j = 0; j < data.NumBins; j++)
            {
                for (int k = 0; k < data.NumRawMaterialOptions; k++)
                {
                    objective.SetCoefficient(y[j, k], DataModel.lengthOptionsRawMaterial[k]);
                }

                for (int i = 0; i < data.NumItems; i++)
                {
                    objective.SetCoefficient(x[i, j], -DataModel.parts[i].Length);
                }
            }

            // Add a term to penalize the number of different sizes used
            for (int j = 0; j < data.NumBins; j++)
            {
                for (int k = 0; k < data.NumRawMaterialOptions; k++)
                {
                    objective.SetCoefficient(y[j, k], penalty);
                }
            }

            objective.SetMinimization();

            Console.WriteLine("Ready to solve.");

            Solver.ResultStatus resultStatus = solver.Solve();

            Console.WriteLine("Solved.");
            // Check that the problem has an optimal solution.
            if (resultStatus != Solver.ResultStatus.OPTIMAL)
            {
                Console.WriteLine("The problem does not have an optimal solution!");
            }
            else if (resultStatus == Solver.ResultStatus.INFEASIBLE)
            {
                // No solution found
                Console.WriteLine("No feasible solution found.");
            }

            Console.WriteLine($"Total Scrap: {solver.Objective().Value()}");

            int howManyTimes = 0;
            List<RawMaterial> rawMaterials = new List<RawMaterial>();

            for (int j = 0; j < data.NumBins; ++j)
            {
                RawMaterial rawMaterial = null;
                for (int i = 0; i < data.NumRawMaterialOptions; i++)
                {
                    if (y[j, i].SolutionValue() == 1)
                    {
                        int binLength = DataModel.lengthOptionsRawMaterial[i];
                        rawMaterial = new RawMaterial(binLength);
                        break;
                    }
                }

                if (rawMaterial != null)
                {
                    for (int i = 0; i < data.NumItems; i++)
                    {
                        if (x[i, j].SolutionValue() == 1)
                        {
                            howManyTimes++;
                            rawMaterial.insert_part(DataModel.parts[i]);
                        }
                    }

                    rawMaterials.Add(rawMaterial);
                }
            }

            Console.WriteLine("How Many Times: " + howManyTimes);
            count_check(rawMaterials);
            rawMaterials.Sort((x, y) => x.Length.CompareTo(y.Length));
            ObservableCollection<RawMaterial> res = new ObservableCollection<RawMaterial>(rawMaterials);
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