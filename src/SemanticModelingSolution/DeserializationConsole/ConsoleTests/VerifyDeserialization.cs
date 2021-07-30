using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DeserializationConsole
{
    public class VerifyDeserialization
    {
        public JsonSerializerOptions SettingsVanilla { get; } = new JsonSerializerOptions()
        {
            WriteIndented = true,
            //DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        };
        
        public JsonSerializerOptions CreateSettings()
            => new JsonSerializerOptions()
            {
                WriteIndented = true,
                Converters =
                {
                    new CodeGenerationLibrary.Serialization.VisualizeTrivialConverter<TestA>(),
                },
            };

        public string GetJson<T>(T item) => JsonSerializer.Serialize(item, SettingsVanilla);

        public object FromJson(string json, Type type, JsonSerializerOptions options) => JsonSerializer.Deserialize(json, type, options);

        public TestA Test()
        {
            var options = CreateSettings();
            var data = GetData();
            var json = GetJson<TestA>(data);
            Console.WriteLine(json);
            return (TestA)FromJson(json, typeof(TestA), options);
        }


        public TestA GetData()
        {
            return new TestA()
            {
                Name1 = "AAA",
                Name2 = null,
                //Name3
                Num1 = 1,
                Num2 = null,
                Dec1 = 1.1m,
                //Dec2
                B1 = true,
                B2 = false,
                B3 = true,
                B4 = false,
                B5 = null,
                //B6
                ChildB = new TestB()
                {
                    AAA = null,
                }
            };
        }

        public class TestA
        {
            public string Name1 { get; set; }
            public string Name2 { get; set; }
            public string Name3 { get; set; }
            public int Num1 { get; set; }
            public int? Num2 { get; set; }
            public decimal Dec1 { get; set;  }
            public decimal Dec2 { get; set;  }
            public bool B1 { get; set; }
            public bool B2 { get; set; }
            public bool? B3 { get; set; }
            public bool? B4 { get; set; }
            public bool? B5 { get; set; }
            public bool? B6 { get; set; }

            public TestB ChildB { get; set; }
        }

        public class TestB
        {
            public string AAA { get; set; }
        }

    }
}
