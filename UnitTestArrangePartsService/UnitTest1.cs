namespace UnitTestArrangePartsService;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        // Arrange
        int expected = 1;
        // Act
        int actual = ReturnOne();

        // Assert
        Assert.Equal(expected, actual);
    }
    
    private int ReturnOne()
    {
        return 1;
    }
}