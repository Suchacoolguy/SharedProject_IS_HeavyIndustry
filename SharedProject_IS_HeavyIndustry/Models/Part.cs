using System.ComponentModel;

namespace SharedProject_IS_HeavyIndustry.Models;

public class Part : INotifyPropertyChanged
{
    public bool is_selected = false;
    private string assem, mark, material;
    private int length, num;
    private double weightOne, weightSum, pArea;
    private Description desc;
    private string descString;
    private bool _isOverLenth, _isExcluded, _needSeparate;
    private string _lengthToBeSeparated;
    public string lengthToBeSeparated
    {
        get => _lengthToBeSeparated;
        set
        {
            if (!int.TryParse(value, out _)) return;
            if (_lengthToBeSeparated == value) return;
            _lengthToBeSeparated = value;
            OnPropertyChanged();
        }
    }
    
    public Part() {}
    
    public Part(string assem, string mark, string material, int length, int num, double weightOne, double weightSum, double pArea, Description desc)
    {
        this.assem = assem;
        this.mark = mark;
        this.material = material;
        this.length = length;
        this.num = num;
        this.weightOne = weightOne;
        this.weightSum = weightSum;
        this.pArea = pArea;
        this.desc = desc;
        descString = desc.ToString();
        _isOverLenth = false;
        _isExcluded = true;
        _needSeparate = false;
        _lengthToBeSeparated = "";
    }
    
    public Description Desc
    {
        get { return desc; }
        set
        {
            desc = value;
            descString = value.ToString();
        }
    }

    public string DescString
    {
        get { return descString; }
        set { descString = value; }
    }
    
    public string Assem
    {
        get { return assem; }
        set { assem = value; }
    }

    public string Mark
    {
        get { return mark; }
        set { mark = value; }
    }

    public string Material
    {
        get { return material; }
        set { material = value; }
    }

    public int Length
    {
        get { return length; }
        set { length = value; }
    }

    public int Num
    {
        get { return num; }
        set { num = value; }
    }

    public double WeightOne
    {
        get { return weightOne; }
        set { weightOne = value; }
    }

    public double WeightSum
    {
        get { return weightSum; }
        set { weightSum = value; }
    }

    public double PArea
    {
        get { return pArea; }
        set { pArea = value; }
    }

    public bool IsOverLenth
    {
        get { return _isOverLenth; }
        set { _isOverLenth = value; }
    }
    
    public bool IsExcluded
    {
        get { return _isExcluded; }
        set
        {
            _isExcluded = value;
            OnPropertyChanged(nameof(IsExcluded));
        }
    }
    
    public bool NeedSeparate
    {
        get { return _needSeparate; }
        set
        {
            _needSeparate = value;
            OnPropertyChanged(nameof(NeedSeparate));
        }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public override string ToString()
    {
        return assem + " " + mark + " " + desc + " " + length + " " + num + " " + weightOne + " " + weightSum + " " +
               pArea + " " + material;
    }

}