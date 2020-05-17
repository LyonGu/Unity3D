using System;
using System.Collections.Generic;

namespace CodeRunner {
    public class CodeRunnerTest {

        public enum Stundent : byte {
            Man,
            Woman
        }

        public static Stundent ty;

        public class Animal {
            public string name;

            private int age;

            public int Age {
                get => age;
                set
                {
                    if (value > 0)
                    {
                        age = value;
                    }

                }
            }

            public void Run () {

            }
        }

        public class Dog : Animal {
            public int GetAge () => this.Age;
        }

        public class Frams<T> where T : Animal {

            private T obj;
            public T GetT () {
                return obj;
            }

            public Frams (T obj1) {
                obj = obj1;
            }
        }

        public class Frams1<T> : List<T> where T : Animal {

        }

        public delegate T1 MyDelegate<T1, T2> (T2 op1, T1 op2) where T1 : T2;

        public class Test<T> : IComparable where T : Animal {
            private T _obj;
            public Test (T obj) {
                _obj = obj;
            }

            public int CompareTo (object obj) {
                if (obj is Animal) {
                    Animal animal = obj as Animal;
                    Animal _animal = _obj as Animal;
                    if (animal.Age <= _animal.Age) {
                        return 1;
                    }
                    return -1;
                }
                return 0;
            }
        }

        static void Main (string[] arges) {
            switch (ty) {
                case Stundent.Man:
                    break;
                case Stundent.Woman:
                    break;

            }

            List<int> intList = new List<int> ();

            //
            /*
                排序列表 按照键值排序，排序列表是数组和哈希表的组合。它包含一个可使用键或索引访问各项的列表。
                如果您使用索引访问各项，则它是一个动态数组（ArrayList），
                如果您使用键访问各项，则它是一个哈希表（Hashtable）。集合中的各项总是按键值排序
            */
            // SortedList<string, int> sortedIntList = new SortedList<string, int> ();
            // sortedIntList.Add ("1", 10);
            // sortedIntList.Add ("0", 0);
            // sortedIntList.Add ("2", 3);
            // foreach (var item in sortedIntList) {
            //     Console.WriteLine (item.Key + "=" + item.Value);
            // }

            // int a = sortedIntList["0"];
            // Console.WriteLine (a);
            // Dictionary<string, string> stringDic = new Dictionary<string, string> () {
            //     ["hxp"] = "1"
            // };
            //SortedDictionary

            Frams<Dog> dogFrams = new Frams<Dog> (new Dog ());
            dogFrams.GetT ().Run ();
        }
    }
}
