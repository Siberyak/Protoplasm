using System.ComponentModel;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var data1 = new TestData1() {StrData1 = "", StrData2 = "222", Dec2 = 11 };
            Obfuscator.Obfuscate(data1);

            var data2 = new TestData2() { StrData1 = "qweqweq", StrData2 = "zxcvv", StrData4 = "cczxcasd"};
            Obfuscator.Obfuscate(data2);

            var data3 = new TestData3() {StrData4 = "asdasd"};
            Obfuscator.Obfuscate(data3);

            data3 = new TestData3() { StrData4 = "asdasdasdasdasda" };
            Obfuscator.Obfuscate(data3);
        }
    }


    [ObfuscationSequence(DisplayName = "'test data 1'", Current = 111000)]
    public class TestData1
    {
        [ObfuscateString]
        public string StrData1 { get; set; }

        [ObfuscateString(Format = " Потртошки № {1}")]
        public string StrData2 { get; set; }
        [ObfuscateString]
        public string StrData3 { get; set; }

        [ObfuscateDecimal]
        public decimal Dec1 { get; set; }

        [ObfuscateDecimal]
        public decimal? Dec2 { get; set; }
    }

    [DisplayName("тестовые данные 2")]
    public class TestData2 : TestData1
    {
        [ObfuscateString]
        public string StrData4 { get; set; }
    }

    public class TestData3
    {
        [ObfuscateString]
        public string StrData4 { get; set; }
    }
}
