using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PathGATSP
{
    class GeneticAlgorithm
    {
        private double minDistance;
        private double rate;
        private double[] x, y;
        private int g;
        private int generations;
        private int number;
        private int population;
        private int[] minTour;
        private Random random;

        public double MinDistance
        {
            get
            {
                return minDistance;
            }
        }

        public int G
        {
            get
            {
                return g;
            }
        }

        public int[] MinTour
        {
            get
            {
                return minTour;
            }
        }

        public GeneticAlgorithm(
            double rate,
            double[] x,
            double[] y,
            int generations,
            int number,
            int population,
            int seed)
        {
            this.rate = rate;
            this.x = x;
            this.y = y;
            this.generations = generations;
            this.number = number;
            this.population = population;

            // evitamos que la función random siempre genere los mismos valores aleatorios
            random = new Random(seed);
        }

        private double Distance(double x1, double y1, double x2, double y2)
        {
            // calcula la distancia euclediana

            return Math.Sqrt(Math.Pow(x1 - x2, 2.0) + Math.Pow(y1 - y2, 2.0));
        }

        private double TourDistance(int[] city)
        {
            double td = 0.0;

            // calcula la distancia total entre todas las ciudades hasta la última
            for (int n = 0; n < number - 1; n++)
                td += Distance(x[city[n]], y[city[n]],
                    x[city[n + 1]], y[city[n + 1]]);

            // añade la distancia del último punto al inicio a la distancia total
            td += Distance(x[city[number - 1]], y[city[number - 1]],
                x[city[0]], y[city[0]]);

            return td;
        }

        // cruce del algoritmo genético
        // cp1 y cp2 son las cotas del arreglo
        // p1 y p2 son los arreglos de dos recorridos específicos
        // o1 y o2 serán los arreglos generados
        private void CruceGenetico(int cp1, int cp2, int[] p1, int[] p2, int[] o1, int[] o2)
        {
            // inicializamos los arreglos generados con -1
            for (int i = 0; i < number; i++)
                o1[i] = o2[i] = -1;

            // los valores comprendidos entre las cotas cp1 y cp2 del recorrido p2 irán al arreglo o1 y del recorrido p1 al arreglo o2
            for (int i = cp1; i <= cp2; i++)
            {
                o1[i] = p2[i];
                o2[i] = p1[i];
            }

            // los valores comprendidos entre 0 y la cota cp1 del primer arreglo generado 
            // serán llenados con los valores del recorrido p1 mientras no genere conflictos
            for (int i = 0; i < cp1; i++)
            {
                bool found = false;
                int t = p1[i];

                for (int j = i + 1; !found && j < number; j++)
                    found = t == o1[j];

                if (!found)
                    o1[i] = t;
            }

            // los valores comprendidos entre la cota cp2 y el límite superior del primer arreglo generado 
            // serán llenados con los valores del recorrido p1 mientras no genere conflictos
            for (int i = cp2 + 1; i < number; i++)
            {
                bool found = false;
                int t = p1[i];

                for (int j = 0; !found && j < number; j++)
                    found = t == o1[j];

                if (!found)
                    o1[i] = t;
            }

            // creo una lista de valores usados y lleno los valores con los números que ya han sido utilizados
            List<int> used = new List<int>();

            for (int i = 0; i < number; i++)
                if (o1[i] != -1)
                    used.Add(o1[i]);

            // creo una lista de valores que no he utilizado para llenar los espacios faltantes del arreglo
            // de una manera más óptima
            List<int> unused = new List<int>();

            for(int i = 0; i < number;i++)
            {
                unused.Add(i);
            }
            for (int i = 0; i < number;i++)
            {
                if (unused.Contains(o1[i]))
                    unused.Remove(o1[i]);
            }

            // se llenan los espacios en el arreglo que no han sido llenados 
            // con valores aleatorios sin generar conflicto

            int index = 0;

            for (int i = 0; i < number; i++)
            {
                if (o1[i] == -1)
                {
                    int x;

                    do
                    {
                        // x = random.Next(number);
                        index = random.Next(unused.Count);
                        x = unused[index];
                    } while (used.Contains(x));
                    unused.RemoveAt(index);

                    o1[i] = x;
                    used.Add(x);
                }
            }


            // se realiza un procedimiento análogo para el arreglo o2
            for (int i = 0; i < cp1; i++)
            {
                bool found = false;
                int t = p2[i];

                for (int j = i + 1; !found && j < number; j++)
                    found = t == o2[j];

                if (!found)
                    o2[i] = t;
            }

            for (int i = cp2 + 1; i < number; i++)
            {
                bool found = false;
                int t = p2[i];

                for (int j = 0; !found && j < number; j++)
                    found = t == o2[j];

                if (!found)
                    o2[i] = t;
            }

            used = new List<int>();

            for (int i = 0; i < number; i++)
                if (o2[i] != -1)
                    used.Add(o2[i]);

            unused = new List<int>();

            for (int i = 0; i < number; i++)
            {
                unused.Add(i);
            }
            for (int i = 0; i < number; i++)
            {
                if (unused.Contains(o2[i]))
                    unused.Remove(o2[i]);
            }

            for (int i = 0; i < number; i++)
            {
                if (o2[i] == -1)
                {
                    int x;

                    do
                    {
                        // x = random.Next(number);
                        index = random.Next(unused.Count);
                        x = unused[index];
                    } while (used.Contains(x));
                    unused.RemoveAt(index);

                    o2[i] = x;
                    used.Add(x);
                }
            }
        }

        public void RunGA(BackgroundWorker bw)
        {
            double[] distance = new double[population];

            minTour = new int[number];

            int[,] chromosome = new int[population, number];

            minDistance = double.MaxValue;

            for (int p = 0; p < population; p++)
            {
                bool[] used = new bool[number];
                int[] city = new int[number];

                for (int n = 0; n < number; n++)
                    used[n] = false;

                for (int n = 0; n < number; n++)
                {
                    int i;

                    do
                    {
                        i = random.Next(number);
                    }
                    while (used[i]);

                    used[i] = true;
                    city[n] = i;
                }

                for (int n = 0; n < number; n++)
                    chromosome[p, n] = city[n];

                distance[p] = TourDistance(city);

                if (distance[p] < minDistance)
                {
                    minDistance = distance[p];

                    for (int n = 0; n < number; n++)
                        minTour[n] = chromosome[p, n];
                }
            }

            for (g = 0; g < generations; g++)
            {
                if (bw.CancellationPending)
                    return;

                if ((g + 1) % 100 == 0)
                    bw.ReportProgress((int)((100.0 * (g + 1)) / generations));

                if (random.NextDouble() < rate)
                {
                    int i, j, parent1, parent2;
                    int[] p1 = new int[number];
                    int[] p2 = new int[number];
                    int[] o1 = new int[number];
                    int[] o2 = new int[number];

                    i = random.Next(population);
                    j = random.Next(population);

                    if (distance[i] < distance[j])
                        parent1 = i;
                    else
                        parent1 = j;

                    i = random.Next(population);
                    j = random.Next(population);

                    if (distance[i] < distance[j])
                        parent2 = i;
                    else
                        parent2 = j;

                    for (i = 0; i < number; i++)
                    {
                        p1[i] = chromosome[parent1, i];
                        p2[i] = chromosome[parent2, i];
                    }

                    int cp1 = -1, cp2 = -1;

                    do
                    {
                        cp1 = random.Next(number);
                        cp2 = random.Next(number);
                    } while (cp1 == cp2 || cp1 > cp2);

                    CruceGenetico(cp1, cp2, p1, p2, o1, o2);

                    double o1Fitness = TourDistance(o1);
                    double o2Fitness = TourDistance(o2);

                    if (o1Fitness < distance[parent1])
                        for (i = 0; i < number; i++)
                            chromosome[parent1, i] = o1[i];

                    if (o2Fitness < distance[parent2])
                        for (i = 0; i < number; i++)
                            chromosome[parent2, i] = o2[i];

                    for (int p = 0; p < population; p++)
                    {
                        if (distance[p] < minDistance)
                        {
                            minDistance = distance[p];

                            for (int n = 0; n < number; n++)
                                minTour[n] = chromosome[p, n];
                        }
                    }
                }

                else
                {
                    int i, j, p;
                    int[] child = new int[number];

                    i = random.Next(population);
                    j = random.Next(population);

                    if (distance[i] < distance[j])
                        p = i;

                    else
                        p = j;

                    double childDistance;

                    for (int n = 0; n < number; n++)
                        child[n] = chromosome[p, n];

                    do
                    {
                        i = random.Next(number);
                        j = random.Next(number);
                    }
                    while (i == j);

                    int t = child[i];

                    child[i] = child[j];
                    child[j] = t;

                    childDistance = TourDistance(child);

                    int maxIndex = int.MaxValue;
                    double maxD = double.MinValue;

                    for (int q = 0; q < population; q++)
                    {
                        if (distance[q] >= maxD)
                        {
                            maxIndex = q;
                            maxD = distance[q];
                        }
                    }

                    int[] index = new int[population];
                    int count = 0;

                    for (int q = 0; q < population; q++)
                    {
                        if (distance[q] == maxD)
                        {
                            index[count++] = q;
                        }
                    }

                    maxIndex = index[random.Next(count)];

                    if (childDistance < distance[maxIndex])
                    {
                        distance[maxIndex] = childDistance;

                        for (int n = 0; n < number; n++)
                            chromosome[maxIndex, n] = child[n];

                        if (childDistance < minDistance)
                        {
                            minDistance = childDistance;

                            for (int n = 0; n < number; n++)
                                minTour[n] = child[n];
                        }
                    }
                }
            }
        }
    }
}