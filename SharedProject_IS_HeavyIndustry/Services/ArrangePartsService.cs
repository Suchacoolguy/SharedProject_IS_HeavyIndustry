using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SharedProject_IS_HeavyIndustry.Models;

namespace SharedProject_IS_HeavyIndustry.Services;

public class ArrangePartsService
{
    private static List<int> _lengthOptionsRawMaterial = new List<int>() {6010, 7010, 7510, 8010, 9510, 10010, 12110};
    private static ObservableCollection<RawMaterial> _rawMaterialsUsed = ArrangeParts();
    
    public static ObservableCollection<RawMaterial> ArrangeParts()
    {
        ObservableCollection<RawMaterial> rawMaterialsUsed = new ObservableCollection<RawMaterial>();
        
        List<Part> partList = ExcelDataLoader.PartListFromExcel("/Users/suchacoolguy/Documents/BOM_test.xlsx");
        // sort in descending order
        partList.Sort((a, b) => b.Length.CompareTo(a.Length));
        // sort in descending order
        _lengthOptionsRawMaterial.Sort((a, b) => b.CompareTo(a));

        foreach (var p in partList)
        {
            Console.Write(p.Length);
            Console.Write(", ");
        }
        
        int bestLength = _lengthOptionsRawMaterial[0];
        // iterate through the list of parts and create raw materials if needed
        for (int i = 0; i < partList.Count; i++)
        {
            
            
            // if no raw material is created yet, create one
            if (rawMaterialsUsed.Count == 0)
            {
                foreach (int rawLength in _lengthOptionsRawMaterial)
                {
                    if (partList[i].Length < rawLength && rawLength < bestLength)
                    {
                        bestLength = rawLength;
                    }
                }
                
                RawMaterial raw = new RawMaterial(bestLength);
                raw.insert_part(partList[i]);
                rawMaterialsUsed.Add(raw);
                
                // removing an item while iterating through the list it belongs to is not recommended, so we don't remove the part here
                // part_list.Remove(part_list[i]);
            }
            // if there are raw materials already created
            else
            {
                bool partAdded = false;
                // search through the raw materials to find the best fit raw material
                // 여기서는 파트가 들어갈 자리가 있능가 보는 던계.
                foreach (var usedRaw in rawMaterialsUsed)
                {
                    // if the part can fit in the raw material, add it
                    if (usedRaw.remaining_length >= partList[i].Length)
                    {
                        usedRaw.insert_part(partList[i]);
                        partAdded = true;
                        break;
                    }
                    
                    // if the part can't fit in the raw material, find the best fit raw material
                    foreach (int rawLength in _lengthOptionsRawMaterial)
                    {
                        if (partList[i].Length <= rawLength && rawLength < bestLength)
                        {
                            bestLength = rawLength;
                        }
                    }
                }
                
                // if the part was not added to any raw material, create a new raw material
                if (partAdded == false)
                {
                    RawMaterial raw = new RawMaterial(bestLength);
                    raw.insert_part(partList[i]);
                    rawMaterialsUsed.Add(raw);
                }
                
                // RawMaterial raw = new RawMaterial(best_length);
                // raw_materials_used.Add(raw);
            }
            
        }
        count_check(rawMaterialsUsed);
        return rawMaterialsUsed;
    }

    public static void OptimizeArrangement()
    {
        
    }

    public int FindBestFitRawMaterial(int partLengthTotal)
    {
        int bestFit = _lengthOptionsRawMaterial[0];
        foreach (var len in _lengthOptionsRawMaterial)
        {
            if (len < bestFit && len - partLengthTotal >= 0)
            {
                bestFit = len;
            }
        }

        return bestFit;
    }

    public bool IsBetterToMove(RawMaterial from, RawMaterial to, Part part)
    {
        // 파트를 이동시키기 전의 from과 to의 스크랩 길이를 구한다.
        int totalScrapBeforeMove = from.remaining_length + to.remaining_length;
        
        // 파트를 이동시킨다면 from 원자재의 스크랩 길이가 얼마나 될지 구한다.
        int fromTotalPartsLengthAfterMove = from.remaining_length - part.Length;
        int fromScrapAfterMove = FindBestFitRawMaterial(fromTotalPartsLengthAfterMove) - fromTotalPartsLengthAfterMove;
        
        // 파트를 이동시킨다면 to 원자재의 스크랩 길이가 얼마나 될지 구한다.
        int toTotalPartsLengthAfterMove = to.remaining_length + part.Length;
        int toScrapAfterMove = FindBestFitRawMaterial(toTotalPartsLengthAfterMove) - toTotalPartsLengthAfterMove;

        // 파트를 이동시키는 것이 더 이득인지 아닌지 판단한다.
        if (fromScrapAfterMove + toScrapAfterMove < totalScrapBeforeMove)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public static RawMaterial garra_creator(List<Part> partList, List<int> partLength, int rawLength)
    {
        RawMaterial raw = new RawMaterial(rawLength);
        
        foreach (var requiredLen in partLength)
        {
            foreach (var part in partList)
            {
                if (part.Length == requiredLen)
                {
                    raw.insert_part(part);
                    partList.Remove(part);
                    break;
                }   
            }
        }    
        return raw;
    }

    public ObservableCollection<RawMaterial> GetArrangedRawMaterials()
    {
        return _rawMaterialsUsed;
    }
    
    public static List<int> GetLengthOptionsRawMaterial()
    {
        return _lengthOptionsRawMaterial;
    }

    public static void count_check(ObservableCollection<RawMaterial> rawMaterialUsed)
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
    }
}