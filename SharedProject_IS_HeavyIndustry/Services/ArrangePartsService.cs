using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.ViewModels;

namespace SharedProject_IS_HeavyIndustry.Services;

public class ArrangePartsService
{
    private static ObservableCollection<RawMaterial?> _rawMaterialsUsed = new();
    private ObservableCollection<Part> _partsCanNotBeArranged = new();

    // Constructor
    public ArrangePartsService(List<Part> parts, ObservableCollection<Part> overSizeParts, List<int> lengthOptions)
    {
        // 파트배치 완료된 것들
        _lengthOptionsRawMaterial = lengthOptions;

        var replacedParts = new List<Part>();

        Console.WriteLine("ArrangePartsService로 넘어온 파트 리스트 개수: " + parts.Count);
        Console.WriteLine("ArrangePartsService로 넘어온 오바사이즈 리스트 개수: " + overSizeParts.Count);

        // 파트배치 하기 전에 분리하는 작업
        foreach (var ppp in overSizeParts)
        {
            Console.WriteLine("*************************분리 길이 ******************\n" + ppp.lengthToBeSeparated);
            // '분리' 체크가 되었는지
            if (ppp.NeedSeparate && !string.IsNullOrEmpty(ppp.lengthToBeSeparated))
            {
                DividePart(ppp, replacedParts);
            }
            else
            {
                // '분리' 체크가 안 되었다면
                if (!ppp.NeedSeparate)
                    _partsCanNotBeArranged.Add(ppp);
                else if (string.IsNullOrEmpty(ppp.lengthToBeSeparated) || Convert.ToInt32(ppp.lengthToBeSeparated) == 0)
                    _partsCanNotBeArranged.Add(ppp);
            }
        }

        parts.AddRange(replacedParts);

        if (parts.Any())
            _rawMaterialsUsed = ArrangeParts(parts);
        else
            Console.WriteLine("No parts to arrange");
    }

    public static List<int> _lengthOptionsRawMaterial { get; set; } = new();

    public ObservableCollection<Part> getPartsCanNotBeArranged()
    {
        return _partsCanNotBeArranged;
    }

    private static void DividePart(Part ppp, List<Part> replacedParts)
    {
        var initLen = ppp.Length;
        foreach (var SeperateLength in ppp.GetSeperateLengthList())
        {
            initLen -= SeperateLength;
            replacedParts.Add(
                new Part(ppp.Assem, ppp.Mark.Insert(0, "J_"), ppp.Material,
                    SeperateLength, ppp.Num, ppp.WeightOne, ppp.WeightSum, ppp.PArea,
                    ppp.Desc)
            );
        }

        replacedParts.Add(
            new Part(ppp.Assem, ppp.Mark.Insert(0, "J_"), ppp.Material,
                initLen, ppp.Num, ppp.WeightOne, ppp.WeightSum, ppp.PArea,
                ppp.Desc)
        );
    }

    public static ObservableCollection<RawMaterial?> ArrangeParts(List<Part> parts)
    {
        var rawMaterialsUsed = new List<RawMaterial>();
        // List<Part> partList = ExcelDataLoader.PartListFromExcel("/Users/suchacoolguy/Documents/BOM_test.xlsx");
        var partList = parts;
        // sort in descending order
        partList.Sort((a, b) => b.Length.CompareTo(a.Length));
        // sort in descending order
        _lengthOptionsRawMaterial.Sort((a, b) => b.CompareTo(a));
        // 블록마크 기준으로 파트들을 그룹화
        var partsByMark = GroupPartsByMark(partList);

        foreach (var markGroup in partsByMark)
        {
            if (CanFitInSingleRawMaterial(markGroup.Value))
            {
                rawMaterialsUsed.Add(CreateRawMaterialWithParts(markGroup.Value));
            }
            else
            {
                if (IsSeparatedMark(markGroup.Key))
                {
                    PlaceSeparatedParts(rawMaterialsUsed, markGroup.Value);
                }
                else
                {
                    rawMaterialsUsed.AddRange(DoTheArrangement(markGroup.Value));
                }
            }
        }

        // Sort raw materials by length
        rawMaterialsUsed.Sort((x, y) => x.Length.CompareTo(y.Length));
        return new ObservableCollection<RawMaterial?>(rawMaterialsUsed);
    }

    private static void PlacePartInAvailableSpace(List<RawMaterial> rawMaterialsUsed, Part part)
    {
        bool placed;
        placed = false;
        // 긴 것들 배치한 원자재들 중에 남는 자리 있으면 거기 넣자
        foreach (var raw in rawMaterialsUsed)
            if (raw.RemainingLength >= part.Length + SettingsViewModel.CuttingLoss && placed == false &&
                raw.PartsInside.Count > 0 &&
                NormalizeMark(raw.PartsInside[0].Mark).Equals(NormalizeMark(part.Mark)))
            {
                raw.insert_part(part);
                placed = true;
                break;
            }

        // 남는 자리가 없으면 새로 만들어서 넣어주는 것이 인지상정~
        if (placed == false) rawMaterialsUsed.AddRange(DoTheArrangement(new List<Part> { part }));
    }

    private static List<RawMaterial> DoTheArrangement(List<Part> partsToBeArranged)
    {
        var res = new List<RawMaterial>();
        
        var bestFitRawMaterial = _lengthOptionsRawMaterial[0];
        var bestFitIndex = 0;
        var partLength = partsToBeArranged[0].Length;

        // 원자재 길이가 하나면 고마 bestFit이고 뭐고 찾을 것도 없지만~ 여러개면 bestFit 찾아야함
        if (_lengthOptionsRawMaterial.Count > 1)
            for (var i = 1; i < _lengthOptionsRawMaterial.Count(); i++)
            {
                var remainingPartsNum = partsToBeArranged.Count;
                var LengthRawMaterial = _lengthOptionsRawMaterial[i];

                var remainingLengthInEach = LengthRawMaterial;

                // 이 원자재 길이로 배치했을 때 하나의 원자재에 몇 개의 파트가 배치될 수 있는지 계산
                var howManyPartsCanFitInEach =
                    ArrangePartsService.howManyPartsCanFitInEach(remainingLengthInEach, partLength);

                if (howManyPartsCanFitInEach == 0)
                    continue;

                var bestScrapSoFar = int.MaxValue;
                var totalScrap = 0;
                var totalPartsLengthInEach = howManyPartsCanFitInEach * partLength +
                                             SettingsViewModel.CuttingLoss * (howManyPartsCanFitInEach - 1);
                var scrapInEachRawMaterial = LengthRawMaterial - totalPartsLengthInEach;
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
                    var totalLengthOfRemainingParts = partLength * remainingPartsNum +
                                                      SettingsViewModel.CuttingLoss * (remainingPartsNum - 1);
                    // 가장 스크랩이 적게 남는 원자재 길이 선정 + 그 스크랩이 얼마인지 결정
                    var remainingScrapForLastParts = LengthRawMaterial - totalLengthOfRemainingParts;
                    foreach (var rawLen in _lengthOptionsRawMaterial)
                        if (rawLen - totalLengthOfRemainingParts < remainingScrapForLastParts)
                            remainingScrapForLastParts = rawLen - totalLengthOfRemainingParts;
                    totalScrap += remainingScrapForLastParts;
                }

                // 토탈 스크랩에 그 스크랩을 추가해서 총 스크랩 길이를 구한다.
                if (totalScrap < bestScrapSoFar) bestFitIndex = i;
            }

        bestFitRawMaterial = _lengthOptionsRawMaterial[bestFitIndex];

        // bestFitRawMaterial을 찾았으므로 이제 배치 시작
        var partsCount = partsToBeArranged.Count;
        var howManyPartsCanFit = howManyPartsCanFitInEach(bestFitRawMaterial, partLength);
        var partIndex = 0;
        while (partsCount / howManyPartsCanFit > 0)
        {
            var rawMaterial = new RawMaterial(bestFitRawMaterial);
            for (var i = 0; i < howManyPartsCanFit; i++)
                if (partIndex < partsToBeArranged.Count)
                {
                    rawMaterial.insert_part(partsToBeArranged[partIndex]);
                    partIndex++;
                }

            res.Add(rawMaterial);
            partsCount -= howManyPartsCanFit;
        }

        // 나머지 파트 배치
        if (partsCount > 0)
        {
            var totalLengthOfRemainingParts =
                partLength * partsCount + SettingsViewModel.CuttingLoss * (partsCount - 1);
            var bestScrapForRemainingParts = int.MaxValue;
            // bestFitRawMaterial 찾기
            foreach (var rawLen in _lengthOptionsRawMaterial)
                if (totalLengthOfRemainingParts <= rawLen &&
                    rawLen - totalLengthOfRemainingParts < bestScrapForRemainingParts)
                {
                    bestScrapForRemainingParts = rawLen - totalLengthOfRemainingParts;
                    bestFitRawMaterial = rawLen;
                }

            var rawMaterial = new RawMaterial(bestFitRawMaterial);
            while (partsCount > 0)
                if (partIndex < partsToBeArranged.Count)
                {
                    rawMaterial.insert_part(partsToBeArranged[partIndex]);
                    partIndex++;
                    partsCount--;
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
            if (!partsByMark.ContainsKey(part.Mark)) partsByMark[part.Mark] = new List<Part>();
            partsByMark[part.Mark].Add(part);
        }

        return partsByMark;
    }

    private static Dictionary<int, List<Part>> GroupPartsByLength(List<Part> parts)
    {
        var partsByLength = new Dictionary<int, List<Part>>();

        foreach (var part in parts)
        {
            if (!partsByLength.ContainsKey(part.Length)) partsByLength[part.Length] = new List<Part>();
            partsByLength[part.Length].Add(part);
        }

        return partsByLength;
    }
    
    private static bool CanFitInSingleRawMaterial(List<Part> parts)
    {
        int totalLength = GetTotalPartsLength(parts);
        return _lengthOptionsRawMaterial.Any(length => length >= totalLength);
    }
    
    private static RawMaterial CreateRawMaterialWithParts(List<Part> parts)
    {
        int bestFitLength = _lengthOptionsRawMaterial.First(length => length >= GetTotalPartsLength(parts));
        var rawMaterial = new RawMaterial(bestFitLength);
    
        foreach (var part in parts)
        {
            rawMaterial.insert_part(part);
        }

        return rawMaterial;
    }

    private static bool IsSeparatedMark(string mark)
    {
        return mark.StartsWith("J_");
    }

    private static void PlaceSeparatedParts(List<RawMaterial> rawMaterialsUsed, List<Part> separatedParts)
    {
        var partsByLength = GroupPartsByLength(separatedParts);

        foreach (var lengthGroup in partsByLength.OrderByDescending(pair => pair.Key))
        {
            foreach (var part in lengthGroup.Value)
            {
                PlacePartInAvailableSpace(rawMaterialsUsed, part);
            }
        }
    }


    private static string NormalizeMark(string mark)
    {
        return mark.StartsWith("J_") ? mark.Substring(2) : mark;
    }

    private static int howManyPartsCanFitInEach(int rawMaterialLength, int partLength)
    {
        var remainingLengthOfRawMaterial = rawMaterialLength;
        var howManyPartsCanFit = 0;
        for (var i = 0; remainingLengthOfRawMaterial - partLength - SettingsViewModel.CuttingLoss * i >= 0; i++)
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
        var totalPartsLength = 0;
        foreach (var part in partList) totalPartsLength += part.Length;

        totalPartsLength += (partList.Count - 1) * SettingsViewModel.CuttingLoss;

        return totalPartsLength;
    }

    public ObservableCollection<RawMaterial?> GetArrangedRawMaterials()
    {
        return _rawMaterialsUsed;
    }

    public static List<int> GetLengthOptionsRawMaterial()
    {
        return _lengthOptionsRawMaterial;
    }
}