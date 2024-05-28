using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SharedProject_IS_HeavyIndustry.Models;
using Google.OrTools.LinearSolver;
namespace SharedProject_IS_HeavyIndustry.Services;

public class ArrangePartsService
{
    private static List<int> _lengthOptionsRawMaterial = new List<int>() {6010, 7010, 7510, 8010, 9510, 10010, 12110};
    private static List<Part> _overSizeParts = new List<Part>();
    private static ObservableCollection<RawMaterial> _rawMaterialsUsed = ArrangeParts();
    
    public static ObservableCollection<RawMaterial> ArrangeParts()
    {
        List<RawMaterial> rawMaterialsUsed = new List<RawMaterial>();
        
        List<Part> partList = ExcelDataLoader.PartListFromExcel(@"/Users/suchacoolguy/Documents/BOM_test.xlsx");
        // sort in descending order
        partList.Sort((a, b) => b.Length.CompareTo(a.Length));
        // sort in descending order
        _lengthOptionsRawMaterial.Sort((a, b) => b.CompareTo(a));

        for (int i=0; i < partList.Count; i++)
        {
            if (partList[i].Length > 10010)
            {
                _overSizeParts.Add(partList[i]);
                partList.Remove(partList[i]);
            }
        }
        
        DataModel data = new DataModel(partList, _lengthOptionsRawMaterial);
        
        // Create the linear solver with the SCIP backend.
        Solver solver = Solver.CreateSolver("CP-SAT");
        solver.SetTimeLimit(10000);
        
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
        Solver.ResultStatus resultStatus = solver.Solve();

        Console.WriteLine("Solved.");
        // Check that the problem has an optimal solution.
        if (resultStatus != Solver.ResultStatus.OPTIMAL)
        {
            Console.WriteLine("The problem does not have an optimal solution!");
            // return;
        }
        Console.WriteLine($"Total Scrap: {solver.Objective().Value()}");
        
        // for (int i = 0; i < data.NumBins; i++)
        // {
        //     int numCheck = 0;
        //     for (int j = 0; j < data.NumRawMaterialOptions; j++)
        //     {
        //         if (y[i, j].SolutionValue() == 1)
        //         {
        //             numCheck++;
        //         }
        //     }
        //     // Console.WriteLine($"Item {i} is in {numCheck} bins.");
        // }
        // Console.WriteLine("Number of bins: " + data.NumBins);

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
        count_check(rawMaterialsUsed);
        rawMaterialsUsed.Sort((x, y) => x.Length.CompareTo(y.Length));
        ObservableCollection<RawMaterial> res = new ObservableCollection<RawMaterial>(rawMaterialsUsed); 
        return res;
    }

    public ObservableCollection<RawMaterial> GetArrangedRawMaterials()
    {
        return _rawMaterialsUsed;
    }
    
    public static List<int> GetLengthOptionsRawMaterial()
    {
        return _lengthOptionsRawMaterial;
    }

    public static void count_check(List<RawMaterial> rawMaterialUsed)
    {
        int count = 0;
        foreach (var raw in rawMaterialUsed)
        {
            foreach (var part in raw.PartsInside)
            {
                count++;
            }
        }
        
        Console.WriteLine("Total parts: " + count);
        Console.WriteLine("Total raw materials: " + rawMaterialUsed.Count);
    }
}