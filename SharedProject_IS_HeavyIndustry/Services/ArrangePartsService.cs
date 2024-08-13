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
    private ObservableCollection<Part> _partsCanNotBeArranged = new ObservableCollection<Part>();

    public ObservableCollection<Part> getPartsCanNotBeArranged()
    {
        return _partsCanNotBeArranged;
    }
    
    // Constructor
    public ArrangePartsService(List<Part> parts, ObservableCollection<Part> overSizeParts, List<int> lengthOptions)
    {
        _partsCanNotBeArranged.Clear();
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
            else
            {
                if (string.IsNullOrEmpty(ppp.lengthToBeSeparated) || Convert.ToInt32(ppp.lengthToBeSeparated) == 0)
                    _partsCanNotBeArranged.Add(ppp);
            }
        }

        _separatedParts = replacedParts;
        foreach (var VARIABLE in _separatedParts)
        {
            Console.WriteLine(VARIABLE);
        }
        parts.AddRange(_separatedParts);
        
        if (parts.Any())
        {
            _rawMaterialsUsed = ArrangeParts(parts);
        }
        else
        {
            Console.WriteLine("No parts to arrange");
        }
        

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
        
        // 블록마크 기준으로 파트들을 그룹화
        Dictionary<string, List<Part>> partsByMark = GroupPartsByMark(partList);

        foreach (var kvp in partsByMark)
        {
            int totalLength = GetTotalPartsLength(kvp.Value);
            
            
            int bestFitRawMaterial = Int32.MaxValue;
            foreach (var rawLength in _lengthOptionsRawMaterial)
            {
                // 블록마크 같은 애들 길이를 다 합쳤을 때 하나의 원자재 길이보다 작은 경우
                if (totalLength / rawLength == 0 && bestFitRawMaterial > rawLength)
                {
                    bestFitRawMaterial = rawLength;
                }
            }
            
            
            // 하나의 원자재에 모두 배치할 수 있다면~
            if (bestFitRawMaterial != Int32.MaxValue)
            {
                // 원자재 생성하고 파트 생성해서 넣어주기
                RawMaterial rawMaterial = new RawMaterial(bestFitRawMaterial);
                foreach (var part in kvp.Value)
                {
                    rawMaterial.insert_part(part);
                }
                rawMaterialsUsed.Add(rawMaterial);
            }
            else
            {
                // DoTheArrangement 함수로 파트 배치하고 Append 하는 작업
                rawMaterialsUsed.AddRange(DoTheArrangement(kvp.Value));
            }
        }
        
        rawMaterialsUsed.Sort((x, y) => x.Length.CompareTo(y.Length));
        ObservableCollection<RawMaterial> res = new ObservableCollection<RawMaterial>(rawMaterialsUsed); 
        return res;
    }

    private static List<RawMaterial> DoTheArrangement(List<Part> partsToBeArranged)
    {
         List<RawMaterial> res = new List<RawMaterial>();
        int bestFitRawMaterial = _lengthOptionsRawMaterial[0];
        int bestFitIndex = 0; int partLength = partsToBeArranged[0].Length;
        
        // 원자재 길이가 하나면 고마 bestFit이고 뭐고 찾을 것도 없지만~ 여러개면 bestFit 찾아야함
        if (_lengthOptionsRawMaterial.Count > 1)
        {
            for (int i = 1; i < _lengthOptionsRawMaterial.Count(); i++)
            {
                int remainingPartsNum = partsToBeArranged.Count;
                int LengthRawMaterial = _lengthOptionsRawMaterial[i];
                
                int remainingLengthInEach = LengthRawMaterial;

                // 이 원자재 길이로 배치했을 때 하나의 원자재에 몇 개의 파트가 배치될 수 있는지 계산
                bool isFirstTime = true;
                int howManyPartsCanFitInEach = ArrangePartsService.howManyPartsCanFitInEach(remainingLengthInEach, partLength);
                
                if (howManyPartsCanFitInEach == 0)
                    continue;
                
                int bestScrapSoFar = Int32.MaxValue;
                int totalScrap = 0;
                int totalPartsLengthInEach = howManyPartsCanFitInEach * partLength + (SettingsViewModel.CuttingLoss * (howManyPartsCanFitInEach - 1));
                int scrapInEachRawMaterial = LengthRawMaterial - totalPartsLengthInEach;
                // 이 원자재 길이로 배치했을 때 총 스크랩 길이를 구한다.
                while (remainingPartsNum / howManyPartsCanFitInEach > 0)
                {
                    // for (int j = 0; j < howManyPartsCanFitInEach; j++)
                    totalScrap += scrapInEachRawMaterial;
                    remainingPartsNum -= howManyPartsCanFitInEach;
                }
                
                // 마지막 남은 파트들을 넣으면?
                if (remainingPartsNum > 0)
                {
                    // 남은 파트 총 길이 계산
                    int totalLengthOfRemainingParts = partLength * remainingPartsNum + (SettingsViewModel.CuttingLoss * (remainingPartsNum - 1));
                    // 가장 스크랩이 적게 남는 원자재 길이 선정 + 그 스크랩이 얼마인지 결정
                    int remainingScrapForLastParts = LengthRawMaterial - totalLengthOfRemainingParts;
                    foreach (var rawLen in _lengthOptionsRawMaterial)
                    {
                        if (rawLen - totalLengthOfRemainingParts < remainingScrapForLastParts)
                        {
                            remainingScrapForLastParts = rawLen - totalLengthOfRemainingParts;
                        }
                    }
                    totalScrap += remainingScrapForLastParts;
                }
                
                // 토탈 스크랩에 그 스크랩을 추가해서 총 스크랩 길이를 구한다.
                if (totalScrap < bestScrapSoFar)
                {
                    bestFitIndex = i;
                }
            }
        }
        bestFitRawMaterial = _lengthOptionsRawMaterial[bestFitIndex];

        // bestFitRawMaterial을 찾았으므로 이제 배치 시작
        int partsCount = partsToBeArranged.Count;
        int howManyPartsCanFit = howManyPartsCanFitInEach(bestFitRawMaterial, partLength);
        int partIndex = 0;
        while (partsCount / howManyPartsCanFit > 0)
        {
            RawMaterial rawMaterial = new RawMaterial(bestFitRawMaterial);
            for (int i = 0; i < howManyPartsCanFit; i++)
            {
                if (partIndex < partsToBeArranged.Count)
                {
                    rawMaterial.insert_part(partsToBeArranged[partIndex]);
                    partIndex++;    
                }
            }
            
            res.Add(rawMaterial);
            partsCount -= howManyPartsCanFit;
        }

        // 나머지 파트 배치
        if (partsCount > 0)
        {
            int totalLengthOfRemainingParts = partLength * partsCount + (SettingsViewModel.CuttingLoss * (partsCount - 1));
            int bestScrapForRemainingParts = Int32.MaxValue;
            // bestFitRawMaterial 찾기
            foreach (var rawLen in _lengthOptionsRawMaterial)
            {
                if (totalLengthOfRemainingParts <= rawLen && rawLen - totalLengthOfRemainingParts < bestScrapForRemainingParts)
                {
                    bestScrapForRemainingParts = rawLen - totalLengthOfRemainingParts;
                    bestFitRawMaterial = rawLen;
                }
            }
            
            RawMaterial rawMaterial = new RawMaterial(bestFitRawMaterial);
            while (partsCount > 0)
            {
                if (partIndex < partsToBeArranged.Count)
                {
                    rawMaterial.insert_part(partsToBeArranged[partIndex]);
                    partIndex++;
                    partsCount--;
                }
            }
            
            res.Add(rawMaterial);
        }

        return res;
    }

    private static Dictionary<string, List<Part>> GroupPartsByMark(List<Part> parts)
    {
        var partsByMark = new Dictionary<string, List<Part>>();

        foreach (var part in parts)
        {
            if (!partsByMark.ContainsKey(part.Mark))
            {
                partsByMark[part.Mark] = new List<Part>();
            }
            partsByMark[part.Mark].Add(part);
        }

        return partsByMark;
    }

    private static int GetSumOfLengthsForKey(List<Part> parts)
    {
        if (parts != null && parts.Count > 0)
        {
            return parts.Sum(part => part.Length);
        }
        return 0;
    }
    
    private static int howManyPartsCanFitInEach(int rawMaterialLength, int partLength)
    {
        int remainingLengthOfRawMaterial = rawMaterialLength;
        int howManyPartsCanFit = 0;
        for (int i = 0; remainingLengthOfRawMaterial - partLength - (SettingsViewModel.CuttingLoss * i) >= 0; i++)
        {
            remainingLengthOfRawMaterial -= partLength;
            
            // 두 번째 부터는 컷팅로스 길이도 추가해야 함.
            if (i != 0)
                remainingLengthOfRawMaterial -= SettingsViewModel.CuttingLoss;
            
            howManyPartsCanFit++;
        }

        return howManyPartsCanFit;
    }

    private static int GetTotalPartsLength(List<Part> partList)
    {
        int totalPartsLength = 0;
        foreach (var part in partList)
        {
            totalPartsLength += part.Length;
        }
        
        totalPartsLength += (partList.Count - 1) * SettingsViewModel.CuttingLoss;

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