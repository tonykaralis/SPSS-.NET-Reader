﻿using NUnit.Framework;
using Spssly.DataReader;
using Spssly.FileParser;
using Spssly.SpssDataset;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Spssly.Tests
{
    [TestFixture]
    public class TestSpssReader
    {
        [Test]
        public void TestReadFile()
        {
            FileStream fileStream = new FileStream(@"TestFiles/test.sav", FileMode.Open, FileAccess.Read, 
                FileShare.Read, 2048*10, FileOptions.SequentialScan);

            int[] varenieValues = {1, 2 ,1};
            string[] streetValues = { "Landsberger Straße", "Fröbelplatz", "Bayerstraße" };

            int varCount;
            int rowCount;
            try
            {
                ReadData(fileStream, out varCount, out rowCount, 
                    new Dictionary<int, Action<int, Variable>>
                    {
                        {0, (i, variable) =>
                        {
                            Assert.AreEqual("varaible ñ", variable.Label, "Label mismatch");
                            Assert.AreEqual(DataType.Numeric, variable.Type, "First file variable should be  a Number");
                        }},
                        {1, (i, variable) =>
                        {
                            Assert.AreEqual("straße", variable.Label, "Label mismatch");
                            Assert.AreEqual(DataType.Text, variable.Type, "Second file variable should be  a text");
                        }}
                    },
                    new Dictionary<int, Action<int, int, Variable, object>>
                    {
                        {0, (r, c, variable, value) =>
                        {   // All numeric values are doubles
                            Assert.IsInstanceOf(typeof(double), value, "First row variable should be a Number");
                            double v = (double) value;
                            Assert.AreEqual(varenieValues[r], v, "Int value is different");
                        }},
                        {1, (r, c, variable, value) =>
                        {
                            Assert.IsInstanceOf(typeof(string), value, "Second row variable should be  a text");
                            string v = (string) value;
                            Assert.AreEqual(streetValues[r], v, "String value is different");
                        }}
                    });
            }
            finally
            {
                fileStream.Close();
            }

            Assert.AreEqual(varCount, 3, "Variable count does not match");
            Assert.AreEqual(rowCount, 3, "Rows count does not match");
        }

        [Test]
        public void TestEmptyStream()
        {
            Assert.Throws<SpssFileFormatException>(() =>ReadData(new MemoryStream(new byte[0]), out int varCount, out int rowCount));
        }

        [Test]
        public void TestReadMissingValuesAsNull()
        {
            FileStream fileStream = new FileStream(@"TestFiles/MissingValues.sav", FileMode.Open, FileAccess.Read,
                FileShare.Read, 2048 * 10, FileOptions.SequentialScan);

            double?[][] varValues =
            {
                new double?[]{ 0, 1, 2, 3, 4, 5, 6, 7 }, // No missing values
                new double?[]{ 0, null, 2, 3, 4, 5, 6, 7 }, // One mssing value
                new double?[]{ 0, null, null, 3, 4, 5, 6, 7 }, // Two missing values
                new double?[]{ 0, null, null, null, 4, 5, 6, 7 }, // Three missing values
                new double?[]{ 0, null, null, null, null, null, 6, 7 }, // Range
                new double?[]{ 0, null, null, null, null, null, 6, null }, // Range & one value
            };

            void rowCheck(int r, int c, Variable variable, object value)
            {
                Assert.AreEqual(varValues[c][r], value, $"Wrong value: row {r}, variable {c}");
            }
            
            try
            {
                ReadData(fileStream, out int varCount, out int rowCount, new Dictionary<int, Action<int, Variable>>
                    {
                        {0, (i, variable) => Assert.AreEqual(MissingValueType.NoMissingValues, variable.MissingValueType)},
                        {1, (i, variable) => Assert.AreEqual(MissingValueType.OneDiscreteMissingValue, variable.MissingValueType)},
                        {2, (i, variable) => Assert.AreEqual(MissingValueType.TwoDiscreteMissingValue, variable.MissingValueType)},
                        {3, (i, variable) => Assert.AreEqual(MissingValueType.ThreeDiscreteMissingValue, variable.MissingValueType)},
                        {4, (i, variable) => Assert.AreEqual(MissingValueType.Range, variable.MissingValueType)},
                        {5, (i, variable) => Assert.AreEqual(MissingValueType.RangeAndDiscrete, variable.MissingValueType)},
                    },
                    new Dictionary<int, Action<int, int, Variable, object>>
                    {
                        {0, rowCheck},
                        {1, rowCheck},
                        {2, rowCheck},
                        {3, rowCheck},
                        {4, rowCheck},
                        {5, rowCheck},
                        {6, rowCheck},
                    });
            }
            finally
            {
                fileStream.Close();
            }
        }

        internal static void ReadData(Stream fileStream, out int varCount, out int rowCount, 
            IDictionary<int, Action<int, Variable>> variableValidators = null, IDictionary<int, Action<int, int, Variable, object>> valueValidators = null)
        {
            SpssReader spssDataset = new SpssReader(fileStream);

            varCount = 0;
            rowCount = 0;

            var variables = spssDataset.Variables;
            foreach (var variable in variables)
            {
                Console.WriteLine("{0} - {1}", variable.Name, variable.Label);
                foreach (KeyValuePair<double, string> label in variable.ValueLabels)
                {
                    Console.WriteLine(" {0} - {1}", label.Key, label.Value);
                }

                if (variableValidators != null && variableValidators.TryGetValue(varCount, out Action<int, Variable> checkVariable))
                {
                    checkVariable(varCount, variable);
                }

                varCount++;
            }

            foreach (var record in spssDataset.Records)
            {
                var varIndex = 0;
                foreach (var variable in variables)
                {
                    Console.Write(variable.Name);
                    Console.Write(':');
                    var value = record.GetValue(variable);
                    Console.Write(value);
                    Console.Write('\t');

                    Action<int, int, Variable, object> checkValue;
                    if (valueValidators != null && valueValidators.TryGetValue(varIndex, out checkValue))
                    {
                        checkValue(rowCount, varIndex, variable, value);
                    }

                    varIndex++;
                }
                Console.WriteLine("");

                rowCount++;
            }
        }
    }
}
