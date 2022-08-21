using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace console_GA
{
    class Program
    {
        private static Random rand = new Random();

        public class Chromosome
        {
            public static double MutationRate = 0.2;
            public static double CrossoverRate = 0.9;
            public static int PopulationSize = 50;
            public static int Lenght = 20;
            public double Value;
            public int[] Gens;

            public Chromosome()
            {
                Value = rand.Next((int)Math.Pow(2, 20));

                BinaryInit();
            }
            public Chromosome(int Value)
            {
                this.Value = Value;

                BinaryInit();
            }

            private void BinaryInit()
            {
                Gens = new int[Lenght];

                char[] binary = Convert.ToString((int)Value, 2).ToCharArray();

                Array.Reverse(binary);

                int index = Gens.Length - 1;
                foreach (char item in binary)
                {
                    Gens[index] = Convert.ToInt32(Convert.ToString(item));
                    index--;
                }

                int converter = 1 << (sizeof(byte) * 20 - 1);
                int ValueConversion = (converter ^ (int)Value) - converter;
                Value = ValueConversion / ((double)Math.Pow(2, 20) - 1) * 12;
            }
        }

        public class Individual
        {
            static int Count = 0;

            public int Number;
            public Chromosome ChromX, ChromY;
            public List<int> Gens;
            public double ObjFunc;
            public double FitnessFunc;

            public Individual()
            {
                ChromX = new Chromosome();
                ChromY = new Chromosome();
                Gens = ChromX.Gens.Concat(ChromY.Gens).ToList();

                Construct();
            }
            public Individual(List<int> BinaryChromosome)
            {
                var BinaryY = new int[Chromosome.Lenght];
                int valueX = Convert.ToInt32(string.Join("", BinaryChromosome.GetRange(0, Chromosome.Lenght)), 2);
                BinaryChromosome.CopyTo(Chromosome.Lenght, BinaryY, 0, Chromosome.Lenght);
                int valueY = Convert.ToInt32(string.Join("", BinaryY), 2);

                ChromX = new Chromosome(valueX);
                ChromY = new Chromosome(valueY);
                Gens = BinaryChromosome;

                Mutate();

                Construct();
            }

            private void Construct()
            {
                Number = ++Count;
                ObjFunc = ObjectiveFunction(ChromX.Value, ChromY.Value);
                FitnessFunc = ObjFunc;
            }

            public void Print()
            {
                Console.Write($" {Number,-3}| ");

                string chromX = $"{ChromX.Value:f5}";
                Console.Write($" {chromX,-9}| ");
                string chromY = $"{ChromY.Value:f5}";
                Console.Write($" {chromY,-9}| ");

                foreach (var bit in Gens)
                    Console.Write(bit);

                Console.Write(" | ");

                string objFunc = $"{ObjFunc:f5}";
                Console.Write($"  {objFunc,-12} | ");

                string fitFunc = $"{FitnessFunc:f5}";
                Console.Write($"   {fitFunc,-12}");

                Console.WriteLine();
            }

            public void Mutate()
            {
                if (rand.NextDouble() < Chromosome.MutationRate)
                {
                    Gens[rand.Next(Gens.Count)] =
                        Convert.ToInt32(!Convert.ToBoolean(Gens[rand.Next(Gens.Count)]));
                }
            }
        }

        public static List<Individual> ParentsCrossing(List<Individual> parents)
        {
            int countChilds = 2;
            var childs = new List<Individual>(countChilds);
            int index = rand.Next(1, parents[0].Gens.Count - 1);

            if (rand.NextDouble() < Chromosome.CrossoverRate)
            {
                var BinaryChromosome = new int[parents[0].Gens.Count];
                parents[0].Gens.CopyTo(0, BinaryChromosome, 0, index);
                parents[1].Gens.CopyTo(index, BinaryChromosome, index, parents[0].Gens.Count - index);
                childs.Add(new Individual(BinaryChromosome.ToList()));

                BinaryChromosome = new int[parents[0].Gens.Count];
                parents[1].Gens.CopyTo(0, BinaryChromosome, 0, index);
                parents[0].Gens.CopyTo(index, BinaryChromosome, index, parents[0].Gens.Count - index);
                childs.Add(new Individual(BinaryChromosome.ToList()));
            }

            return childs;
        }

        public static List<Individual> ChooseTheBestParents(List<Individual> individuals)
        {
            var sortedIndividuals = individuals.OrderByDescending(i => i.FitnessFunc);

            var parents = new List<Individual> { sortedIndividuals.ToList()[0], sortedIndividuals.ToList()[1] };

            return parents;
        }

        private static void ChooseNewIndividuals(ref List<Individual> individuals)
        {
            int count = individuals.Count / 2;
            var newIndividuals = new List<Individual>(count);

            for (int i = 0; i < count; i++)
            {
                int firstFighterIndex = rand.Next(individuals.Count);
                Individual firstFighter = individuals[firstFighterIndex];
                individuals.RemoveAt(firstFighterIndex);

                int secondFighterIndex = rand.Next(individuals.Count);
                Individual secondFighter = individuals[secondFighterIndex];
                individuals.RemoveAt(secondFighterIndex);

                if (firstFighter.FitnessFunc > secondFighter.FitnessFunc)
                {
                    firstFighter.Number = i + 1;
                    newIndividuals.Add(firstFighter);
                }
                else
                {
                    secondFighter.Number = i + 1;
                    newIndividuals.Add(secondFighter);
                }
            }

            individuals = newIndividuals;
        }

        private static void Run(ref List<Individual> individuals)
        {
            List<Individual> parents = ChooseTheBestParents(individuals);

            for (int i = 0; i < 25; i++)
            {
                foreach (var child in ParentsCrossing(parents))
                    individuals.Add(child);
            }

            ChooseNewIndividuals(ref individuals);
        }

        public static double ObjectiveFunction(params double[] values)
        {
            double x = values[0];
            double y = values[1];

            return (0.1 * x + 0.1 * y - 4 * Math.Cos(0.8 * x) + 4 * Math.Cos(0.8 * y) + 8);
        }

        private static void PrintAllIndividuals(List<Individual> individuals)
        {
            Console.WriteLine($" {"№",-3}| x-фенотип | у-фенотип |               XY-хромосома               | Целевая f(x,y) | Приспособ F(x,y)");

            foreach (var individual in individuals)
            {
                individual.Print();
            }
        }

        static void Main(string[] args)
        {
            var individuals = new List<Individual>();

            for (int i = 0; i < 50; i++)
            {
                individuals.Add(new Individual());
            }

            for (int i = 0; i < 10000; i++)
            {
                if (i == 0 || i == 3)
                    PrintAllIndividuals(individuals);

                Run(ref individuals);
            }

            PrintAllIndividuals(individuals);
        }
    }
}
