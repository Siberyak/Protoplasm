using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Protoplasm.Graph;
using Protoplasm.Graph;
using Protoplasm.ViewsAndActions;

namespace Protoplasm.Viewer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Loader.Load("staticdata.json");


            var gr = GetTestData2();
            gr.Datas<TestData1>();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Application.Run(new Form1(InitMVC, GetTestData(), "root"));
            Application.Run(new Form1(InitMVC2, GetTestData2()));
        }

        private static void InitMVC2(ModelViewController mvc, ViewHelper vh)
        {
            mvc.Registrator<MutableDataGraph>()
                .ViewsRegistrator()
                .AsEntity()
                .Detail(x => x.DataNodes<TestData1>().Cast<DataNode<TestData1>>().ToArray(), "", MasterDetails.Vertical);

            mvc.Registrator<DataNode<TestData1>[]>()
                .ViewsRegistrator()
                .AsCollection()
                .ViewGrid();

            mvc.Registrator<DataNode<TestData1>>()
                .ViewsRegistrator()
                .AsEntity()
                .Detail(x => x.ReferencedNodes<IDataEdge<EdgeType>, DataNode<TestData2>>(e => e.Data == EdgeType.ParentChild).ToArray(), "Childs", MasterDetails.Vertical)
                ;

            mvc.Registrator<DataNode<TestData2>[]>()
                .ViewsRegistrator()
                .AsCollection()
                .ViewGrid();

            mvc.Registrator<DataNode<TestData2>>()
                .ViewsRegistrator()
                .AsEntity()
                .Properties(x => $"Properties of '{x.Data.Caption}'");


        }

        private static void InitMVC(ModelViewController mvc, ViewHelper vh)
        {

            mvc.Registrator<List<TestData1>>()
                .ViewsRegistrator("root")
                .AsEntity()
                .Detail(x => x, MasterDetails.Horizontal, ModelViewController.DefaultViewContext);

            mvc.Registrator<List<TestData1>>()
                .ViewsRegistrator()
                .AsCollection()
                .ViewGrid();

            mvc.Registrator<TestData1>()
                .ViewsRegistrator()
                .AsEntity()
                .Detail(x => x.Childs, MasterDetails.Vertical);

            mvc.Registrator<List<TestData2>>()
                .ViewsRegistrator()
                .AsCollection()
                .ViewGrid();

            mvc.Registrator<TestData2>()
                .ViewsRegistrator()
                .AsEntity()
                .Properties();

        }

        private static MutableDataGraph GetTestData2()
        {
            var graph = new MutableDataGraph();

            var td1s = GetTestData();

            foreach (var testData1 in td1s)
            {
                var node = graph.Add(testData1);
                foreach (var child in testData1.Childs)
                {
                    var childNode = graph.Add(child);
                    graph.Add(node, childNode, EdgeType.ParentChild);
                }
            }

            return graph;
        }

        private static List<TestData1> GetTestData()
        {
            return new List<TestData1>()
            {
                new TestData1("1", "1.1", "1.2"),
                new TestData1("2", "2.1", "2.2", "2.3"),
                new TestData1("3"),
                new TestData1("4", "4.1"),
            };
        }
    }


    public class TestData1
    {
        public TestData1(string caption, params string[] childs)
        {
            Caption = caption;
            Childs = childs.Select(x => new TestData2() {Caption = x}).ToList();
        }

        public string Caption { get; }

        [Browsable(false)]
        public List<TestData2> Childs { get; }
    }

    public class TestData2
    {
        public string Caption { get; set; }
    }


    public class TestData3
    {
        public string Caption { get; set; }
    }

    public enum EdgeType
    {
        ParentChild, Referenced
    }
}
