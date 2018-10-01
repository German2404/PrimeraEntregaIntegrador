using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Concurrent;
using System.Threading;

namespace PrimeraEntregaIntegrador
{
    class Analyzer
    {


        public Dictionary<String, Transaction> transactions;
        public Dictionary<String, Client> clients;
        public SortedSet<String> items;
        public double supportThreshold;
        public double confidenceThreshold;

        public Analyzer(double supportThreshold, double confidenceThreshold)
        {
            this.supportThreshold = supportThreshold;
            this.confidenceThreshold = confidenceThreshold;
            this.transactions = new Dictionary<string, Transaction>();
            this.clients = new Dictionary<string, Client>();
            this.items = new SortedSet<string>();
        }


        public void readTransactions(String path)
        {
            String[] lines = File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; i++)
            {
                String[] split = lines[i].Split(' ');
                if (transactions.ContainsKey(split[0]))
                {
                    items.Add(split[3]);
                    transactions[split[0]].items.Add(split[3]);
                }
                else
                {
                    String[] dateSplit = split[2].Split('/');
                    items.Add(split[3]);
                    DateTime date = new DateTime(Int32.Parse(dateSplit[2]), Int32.Parse(dateSplit[1]), Int32.Parse(dateSplit[0]));
                    transactions[split[0]] = new Transaction(split[0], split[1], date, split[3]);
                }
            }
        }


        private List<Transaction> prune(int infLimItems, int supLimItems, int infLimClients, int supLimClients)
        {
            var initial = transactions.Select(i => i.Value).Where(i => i.items.Count <= supLimItems && i.items.Count >= infLimItems);
            var grouping = initial.GroupBy(i => i.clientCode).Where(g => g.Count() >= infLimClients && g.Count() <= supLimClients);
            List<Transaction> filtered = new List<Transaction>();
            foreach( var g in grouping)
            {
                foreach( var h in g)
                {
                    filtered.Add(h);
                }

            }
            return filtered;
        }


        private List<Association> generateBruteForceAssotiations(List<Transaction> prunnedTransactions)
        {
            List<Association> answer = new List<Association>();
            String[] items = prunnedTransactions.SelectMany(i => i.items).Distinct().ToArray();
       
            //Console.WriteLine("Refined Items:" + items.Length);
            var powerSet = PowerSetGenerator.FastPowerSet<String>(items);
       
            foreach(String[] c in powerSet)
            {
                String[] difference = items.Except(c).ToArray();
                var auxPowerSet = PowerSetGenerator.FastPowerSet<String>(difference);
                foreach (String[]c2 in auxPowerSet)
                {
                    answer.Add(new Association(new SortedSet<string>(c), new SortedSet<string>(c2)));
                }
            }
            return answer;

        }


        private double calculateAssociationSupport(Association a,List<Transaction>prunnedTransactions)
        {
            String[] union = a.from.Union(a.to).ToArray();
            int cont = 0;
            foreach(Transaction t in prunnedTransactions)
            {
                bool contains = !union.Except(t.items).Any();
                if (contains)
                {
                    cont++;
                }
            }
            return (double)cont / prunnedTransactions.Count;
        }

        private double calculateAssociationConfidence(Association a, List<Transaction> prunnedTransactions)
        {
            String[] union = a.from.Union(a.to).ToArray();
            int cont1 = 0;
            int cont2 = 0;
            foreach(Transaction t in prunnedTransactions)
            {
                bool contains = !union.Except(t.items).Any();
                if (contains)
                {
                    cont1++;
                }
                bool containsX = !a.from.Except(t.items).Any();
                if (containsX)
                {
                    cont2++;
                }
            }
           
            return (double)cont1 / cont2;
        }

        private List<Association> refineAssociations(List<Association> a, List<Transaction> prunnedTransactions)
        {
            List<Association> answer = new List<Association>();
            for (int i = 0; i < a.Count; i++)
            {
                double s = this.calculateAssociationSupport(a[i], prunnedTransactions);
                double c = this.calculateAssociationConfidence(a[i], prunnedTransactions);
                if(s>=this.supportThreshold && c >= this.confidenceThreshold)
                {
                //Console.WriteLine(a[i] + " " + c);
                    answer.Add(a[i]);
                }
            }
            return answer;
        }

        private double calculateItemSetSupport(List<Transaction> transactions, SortedSet<String> itemSet)
        {
            double count = 0;
            foreach (Transaction t in transactions)
            {
                if (itemSet.IsSubsetOf(t.items))
                {
                    //Console.WriteLine("{" + String.Join(",", itemSet) + "}-->{" + string.Join(",", t.getItems()) + "} SI");
                    count++;
                }
                //else
                //{
                //    Console.WriteLine("{" + String.Join(",", itemSet) + "}-->{" + string.Join(",", t.getItems()) + "} NO");
                //}
            }
            return count / transactions.Count;
        }

        public List<Association>giveBruteForceRefinedAssotiations(int infLimItems, int supLimItems, int infLimClients, int supLimClients)
        {
            var list = this.prune(infLimItems, supLimItems, infLimClients, supLimClients);
            //Console.WriteLine("Total transactions:" + this.transactions.Count);
            //Console.WriteLine("Total items:" + this.items.Count);
            //Console.WriteLine("Refined transactions:" + list.Count);
            var a = this.generateBruteForceAssotiations(list);
            return this.refineAssociations(a, list);
        }


        private List<SortedSet<String>> giveAPrioriFrequentItemsSets(int infLimItems, int supLimItems, int infLimClients, int supLimClients,List<Transaction> transactions)
        {

            String[] items = transactions.SelectMany(i => i.items).Distinct().ToArray();
            //Console.WriteLine("Total items:" + this.items.Count);
            //Console.WriteLine("Refined items:" + items.Length);
            //Console.WriteLine("Nivel de arbol:" + 1);
            List<SortedSet<String>> infrequentItemSets = new List<SortedSet<string>>();

            var frequentItemsSets = new List<SortedSet<String>>();
            List<SortedSet<String>> lastLevel = new List<SortedSet<String>>();

            String[] itemsSku = items;

            ConcurrentBag<SortedSet<String>> unfrequentBag = new ConcurrentBag<SortedSet<string>>();
            ConcurrentBag<SortedSet<String>> frequentBag = new ConcurrentBag<SortedSet<string>>();
            ConcurrentBag<SortedSet<String>> actualBag = new ConcurrentBag<SortedSet<string>>();
            Parallel.For(0, itemsSku.Length, i =>
            {

                SortedSet<String> hash = new SortedSet<string>();
                hash.Add(itemsSku[i]);


                double count = calculateItemSetSupport(transactions, hash);
              
                if (count < this.supportThreshold)
                {
                    unfrequentBag.Add(hash);
                }
                else
                {

                    frequentBag.Add(hash);
                    actualBag.Add(hash);
                }
            });
            infrequentItemSets.AddRange(unfrequentBag);
            frequentItemsSets.AddRange(frequentBag);
            lastLevel.AddRange(actualBag);

            //for (int i = 0; i < itemsSku.Length; i++)
            //{
            //    SortedSet<String> hash = new SortedSet<string>();
            //    hash.Add(itemsSku[i]);


            //    double count = supportCount(transactions, hash);

            //    if (count < st)
            //    {
            //        infrequentItemSets.Add(hash);
            //    }
            //    else
            //    {

            //        frequentItemsSets.Add(hash);
            //        lastLevel.Add(hash);
            //    }
            //}

            //Console.WriteLine("Frequent itemsSet count at level {0}:{1}", 1, frequentItemsSets.Count);
            List<SortedSet<String>> actual = new List<SortedSet<string>>();
            for (int i = 0; i < lastLevel.Count; i++)
            {
                for (int j = i + 1; j < lastLevel.Count; j++)
                {
                    SortedSet<String> set = new SortedSet<string>(lastLevel[i].Union(lastLevel[j]));
                    double sp = calculateItemSetSupport(transactions, set);
                    if (sp < this.supportThreshold)
                    {
                        infrequentItemSets.Add(set);
                    }
                    else
                    {
                        actual.Add(set);
                        frequentItemsSets.Add(set);


                    }
                }
            }
            //Console.WriteLine("Frequent itemsSet count at level {0}:{1}", 2, frequentItemsSets.Count);
            lastLevel = actual;





            for (int i = 3; i <= items.Length; i++)
            {

                List<SortedSet<String>> actualLevel = new List<SortedSet<string>>();
                //Console.WriteLine("Nivel de arbol:" + i);
                for (int j = 0; j < lastLevel.Count; j++)
                {
                    for (int k = j + 1; k < lastLevel.Count; k++)
                    {
                        if (lastLevel[k].Take(lastLevel[k].Count - 1).SequenceEqual(lastLevel[j].Take(lastLevel[j].Count - 1)) && !lastLevel[k].ElementAt(lastLevel[k].Count - 1).Equals(lastLevel[j].ElementAt(lastLevel[j].Count - 1)))
                        {
                            SortedSet<String> candidate = new SortedSet<String>(lastLevel[k].Union(lastLevel[j]));

                            //Console.WriteLine(!infrequentItemSets.Any(inf => { Console.WriteLine("{" + String.Join(",", candidate) + "}-->{" + string.Join(",", inf) + "}"); return candidate.IsSupersetOf(inf); }));

                            if (candidate.Count == i && !infrequentItemSets.Any(inf => candidate.IsSupersetOf(inf)) && calculateItemSetSupport(transactions, candidate) >= this.supportThreshold)
                            {

                                frequentItemsSets.Add(candidate);
                                actualLevel.Add(candidate);
                            }
                            else
                            {
                                infrequentItemSets.Add(candidate);
                            }
                        }



                    }
                }
                lastLevel = actualLevel;
                //Console.WriteLine("Frequent itemsSet count at level {0}:{1}", i, frequentItemsSets.Count);

            }

            return frequentItemsSets;
        }

        public List<Association>apriori2(int infLimItems, int supLimItems, int infLimClients, int supLimClients)
        {
            var prunnedTransactions = this.prune(infLimItems, supLimItems, infLimClients, supLimClients);
            //Console.WriteLine("Transacciones refinadas:" + prunnedTransactions.Count);
            var fItemSets = this.giveAPrioriFrequentItemsSets(infLimItems, supLimItems, infLimClients, supLimClients, prunnedTransactions);
            
            foreach (var itemset in fItemSets)
            {
                List<Association[]> niveles = new List<Association[]>();
                List<Association> originales = new List<Association>();
                for (int i = 0; i < itemset.Count; i++)
                {
                    String[] item = new String[] { itemset.ElementAt(i) };
                    Association a = new Association(new SortedSet<string>(itemset.Except(item)), new SortedSet<String>(item));
                    double c = this.calculateAssociationConfidence(a, prunnedTransactions);

                    if (c >= this.confidenceThreshold)
                    {

                        originales.Add(a);
                    }
                    
                }
                niveles.Add(originales.ToArray());
                int nivelActual = 0;
                while (nivelActual<itemset.Count-1)
                {
                    for (int i = 0; i < 0; i++)
                    {

                    }
                }
            }
            return null;
        }


        public List<Association> giveAPrioriRefinedAssotiations(int infLimItems, int supLimItems, int infLimClients, int supLimClients)
        {
            //Console.WriteLine("Transacciones totales:" + this.transactions.Count);
            var prunnedTransactions = this.prune(infLimItems, supLimItems, infLimClients, supLimClients);
            //Console.WriteLine("Transacciones refinadas:" + prunnedTransactions.Count);
            var fItemSets = this.giveAPrioriFrequentItemsSets(infLimItems, supLimItems, infLimClients, supLimClients,prunnedTransactions);
          
            //Console.WriteLine("ItemSets generated");
            //foreach(var its in fItemSets)
            //{
            //    Console.WriteLine("{"+String.Join(",", its)+"}");
            //}
            List<Association> answer = new List<Association>();
            
            for (int i = 0; i < fItemSets.Count; i++)
            {
                List<List<Association>> levels = new List<List<Association>>();
                if (fItemSets[i].Count > 1)
                {
                   
                    var discardedConsecuents = new List<SortedSet<String>>();
                    var lastLevel = new List<Association>();
                    String[] itemSet = fItemSets[i].ToArray();
                    for (int j = 0; j < fItemSets[i].Count; j++)
                    {
                        
                        String[] item = new String[] { fItemSets[i].ElementAt(j) };
                        Association a = new Association(new SortedSet<string>(itemSet.Except(item)), new SortedSet<String>(item));
                        
                        double c = this.calculateAssociationConfidence(a, prunnedTransactions);
           
                        if (c >= this.confidenceThreshold)
                        {
                           
                            answer.Add(a);
                            lastLevel.Add(a);
                        }
                        else
                        {
                            
                            discardedConsecuents.Add(a.to);
                        }
                    }
                    levels.Add(lastLevel);
                    int antecedentLevel = fItemSets[i].Count - 1;
                    int currentLevel = 0;

                    
                    
                 
                    while (antecedentLevel > 1)
                    {
                        List<Association> actualLevel = new List<Association>();

                        
                        for (int k = 0; k < levels[currentLevel].Count ; k++)
                        {
                            for (int z = k; z < levels[currentLevel].Count; z++)
                            {
                                if (z == k && k%3==0)
                                {
                                    Console.WriteLine(k);
                                }

                                if (k != z)
                                {
                                    
                                    String[] from1 = levels[currentLevel][k].from.ToArray();
                                    String[] from2 = levels[currentLevel][z].from.ToArray();
                                    String[] to1 = levels[currentLevel][k].to.ToArray();
                                    String[] to2 = levels[currentLevel][z].to.ToArray();
                                    String[] inter = from1.Intersect(from2).ToArray();
                                    
                                    if (inter.Count() > 0)
                                    {
                                        String[] union = to1.Union(to2).ToArray();
                                        //Console.WriteLine("{" + String.Join(",", to1) + "}");
                                        //Console.WriteLine("{" + String.Join(",", to2) + "}");
                                        //Console.WriteLine("{" + String.Join(",", union) + "}"+"\n");
                                        
                                        Association candidate = new Association(new SortedSet<string>(inter), new SortedSet<string>(union));
                                        
                                        if (discardedConsecuents.Any(d => candidate.to.IsSubsetOf(d)))
                                        {
                                            
                                            discardedConsecuents.Add(candidate.to);

                                     
                                        }
                                        else
                                        {
                                            
                                            answer.Add(candidate);
                                           
                                            actualLevel.Add(candidate);
                                            
                                        }
                                    }
                                }
                            }
                        }
                        
                        
                        antecedentLevel--;
                        
                        levels.Add(actualLevel);
                        currentLevel++;
                    }


                }
                
            }

            for (int i = 0; i < answer.Count; i++)
            {
                for (int j = i+1; j < answer.Count; j++)
                {
                    if (answer[i].Equals(answer[j]))
                    {
                        answer.RemoveAt(j);
                        j--;
                        
                    }
                }
            }



            return answer.Where(i=>this.calculateAssociationConfidence(i,prunnedTransactions)>=this.confidenceThreshold).ToList();

        }
    }
}
