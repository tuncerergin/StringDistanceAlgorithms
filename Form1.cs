using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Diagnostics;
using F23.StringSimilarity;
using FuzzyString;
using System.IO;
using Code7248.word_reader;

namespace Kelime
{
    public partial class Form1 : Form
    {
        static string tSql;

        static SQLiteConnection tCon = new SQLiteConnection("Data Source=data.db;Version=3;New=True;Compress=True;");

        //SQLiteConnection bag = new SQLiteConnection("Data Source=data.db"); // Debug Klasörümüzdeki database Dosyamızın  adını yazdık veritabanıadi.s3db gibi
        //SQLiteConnection yeni = new SQLiteConnection(bag);
        SQLiteCommand cmd = new SQLiteCommand(tSql, tCon);

        private SQLiteDataReader tDataReader;
        private static readonly double mWeightThreshold = 0.7;
        private static readonly int mNumChars = 4;


        public Form1()
        {
            InitializeComponent();
        }

        double tToplamHamming,
            tToplamJaccard,
            tToplamEuc,
            tToplamJaro,
            tToplamJaroWink,
            tToplamLevens,
            tToplamNgram,
            tToplamQgram,
            tToplamLcs;

        double hesap;
        int gelenHamming, gelenCalcLevenshteinDistance, sayac = 1;
        double gelenJaccard;

        ArrayList aList = new ArrayList();

        static double prgHesap;

        int adetHamming, adetJaccard, adetEuc, adetJaro, adetJaroWink, adetLevens, adetNgram, adetQgram, adetLcs;

        private void button1_Click(object sender, EventArgs e)
        {
            //hamming();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        public void uzunluk_hesabi()
        {
            tSql = "select kelime from kelimeler";
            tCon.Open();
            cmd.CommandText = tSql;

            tDataReader = cmd.ExecuteReader();

            prgHesap = tDataReader.Cast<object>().Count();

            tDataReader.Close();
            bag_close();
        }

        public void hamming()
        {
            sayac = 0;
            kelimeleri_ayir();
            dgrTab1.Rows.Clear();

            tSql = "select kelime from kelimeler";
            tCon.Open();
            cmd.CommandText = tSql;
            tDataReader = cmd.ExecuteReader();

            while (tDataReader.Read())
            {
                sayac++;
                prgGenel.Value = Convert.ToInt32((100 * sayac) / prgHesap);

                foreach (string eleman in aList)
                {
                    if (eleman.Length == tDataReader["kelime"].ToString().Length)
                    {
                        gelenHamming = GetHammingDistance(eleman, tDataReader["kelime"].ToString());
                        hesap = (eleman.Length - gelenHamming) * (100 / eleman.Length);
                        if (hesap > 20)
                        {
                            dgrTab1.Rows.Add(eleman, hesap, tDataReader["kelime"].ToString());
                            tToplamHamming = tToplamHamming + hesap;
                            adetHamming += 1;
                        }
                    }
                }
            }

            tDataReader.Close();
            bag_close();

            dgrTab1.Sort(dgrTab1.Columns[1], ListSortDirection.Descending);


            bprgHamming.Value = Convert.ToInt32(tToplamHamming / Convert.ToDouble(adetHamming));
            hamming_sonuc();
        }

        private void chkTamami_CheckedChanged(object sender, EventArgs e)
        {
        }

        public void Jaccard()
        {
            sayac = 0;
            kelimeleri_ayir();
            dgrTab2.Rows.Clear();

            tSql = "select kelime from kelimeler";
            tCon.Open();
            cmd.CommandText = tSql;
            tDataReader = cmd.ExecuteReader();
            while (tDataReader.Read())
            {
                sayac++;
                prgGenel.Value = Convert.ToInt32((100 * sayac) / prgHesap);
                foreach (string eleman in aList)
                {
                    try
                    {  gelenJaccard = JaccardTest1(eleman, tDataReader["kelime"].ToString());
                    hesap = (gelenJaccard) * (100 / eleman.Length);
                    if (hesap > 20)
                    {
                        dgrTab2.Rows.Add(eleman, hesap, tDataReader["kelime"].ToString());
                        tToplamJaccard += hesap;
                        adetJaccard += 1;
                    }

                    }
                    catch (Exception e)
                    {
                        
                        }
                  
                }
            }

            tDataReader.Close();
            bag_close();

            dgrTab2.Sort(dgrTab2.Columns[1], ListSortDirection.Descending);

            //lblJaccard.Text = tToplamJaccard / adetJaccard + "%";
            //prgJaccard.Value = Convert.ToInt32(tToplamJaccard / Convert.ToDouble(adetJaccard));
            bprgJaccard.Value = Convert.ToInt32(tToplamJaccard / Convert.ToDouble(adetJaccard));
            jaccard_sonuc();
        }

        public void Jaro()
        {
            sayac = 0;
            kelimeleri_ayir();
            dgrTab4.Rows.Clear();

            JaroDistance jaroDistance = new JaroDistance();
            tSql = "select kelime from kelimeler";
            tCon.Open();
            cmd.CommandText = tSql;
            tDataReader = cmd.ExecuteReader();
            while (tDataReader.Read())
            {
                sayac++;
                prgGenel.Value = Convert.ToInt32((100 * sayac) / prgHesap);

                foreach (string eleman in aList)
                {
                    hesap = Convert.ToInt32(jaroDistance.GetDistance(eleman, tDataReader["kelime"].ToString()) * 100);

                    if (hesap > 20)
                    {
                        dgrTab4.Rows.Add(eleman, (hesap), tDataReader["kelime"].ToString());
                        tToplamJaro += hesap;
                        adetJaro += 1;
                    }
                }
            }

            tDataReader.Close();
            bag_close();


            dgrTab4.Sort(dgrTab4.Columns[1], ListSortDirection.Descending);

            //lblJaro.Text = tToplamJaro / adetJaro + "%";
            //prgJaro.Value = Convert.ToInt32(tToplamJaro / Convert.ToDouble(adetJaro));
            bprgJaro.Value = Convert.ToInt32(tToplamJaro / Convert.ToDouble(adetJaro));

            jaro_sonuc();
        }

        public void jaro_wink()
        {
            sayac = 0;
            kelimeleri_ayir();
            dgrTab5.Rows.Clear();

            JaroWinklerDistance jaroWinkler = new JaroWinklerDistance();

            tSql = "select kelime from kelimeler";
            tCon.Open();
            cmd.CommandText = tSql;
            tDataReader = cmd.ExecuteReader();
            while (tDataReader.Read())
            {
                sayac++;
                prgGenel.Value = Convert.ToInt32((100 * sayac) / prgHesap);

                foreach (string eleman in aList)
                {
                    hesap = jaroWinkler.GetDistance(eleman, tDataReader["kelime"].ToString()) * 100;
                    if (hesap > 20)
                    {
                        dgrTab5.Rows.Add(eleman, hesap, tDataReader["kelime"].ToString());
                        tToplamJaroWink += hesap;
                        adetJaroWink += 1;
                    }
                }
            }

            tDataReader.Close();
            bag_close();

            dgrTab5.Sort(dgrTab5.Columns[1], ListSortDirection.Descending);

            //lblJaroWink.Text = tToplamJaroWink / adetJaroWink + "%";
            //prgJaroWink.Value = Convert.ToInt32(tToplamJaroWink / Convert.ToDouble(adetJaroWink));
            bprgJarowiki.Value = Convert.ToInt32(tToplamJaroWink / Convert.ToDouble(adetJaroWink));

            jarowink_sonuc();
        }

        public void Levenshtein()
        {
            sayac = 0;
            kelimeleri_ayir();
            dgrTab6.Rows.Clear();

            LevenshteinDistance levenstein = new LevenshteinDistance();

            tSql = "select kelime from kelimeler";
            tCon.Open();
            cmd.CommandText = tSql;
            tDataReader = cmd.ExecuteReader();
            while (tDataReader.Read())
            {
                sayac++;
                prgGenel.Value = Convert.ToInt32((100 * sayac) / prgHesap);

                foreach (string eleman in aList)
                {
                    hesap = Convert.ToInt32(levenstein.GetDistance(eleman, tDataReader["kelime"].ToString()) * 100);
                    if (hesap > 20)
                    {
                        dgrTab6.Rows.Add(eleman, hesap, tDataReader["kelime"].ToString());
                        tToplamLevens += hesap;
                        adetLevens += 1;
                    }
                }
            }

            tDataReader.Close();
            bag_close();

            dgrTab6.Sort(dgrTab6.Columns[1], ListSortDirection.Descending);

            //lblLevenshtein.Text = tToplamLevens / adetLevens + "%";
            //prgLevens.Value = Convert.ToInt32(tToplamLevens / Convert.ToDouble(adetLevens));
            bprgLevens.Value = Convert.ToInt32(tToplamLevens / Convert.ToDouble(adetLevens));
            levens_sonuc();
        }

        public void n_gram()
        {
            sayac = 0;
            kelimeleri_ayir();
            dgrTab7.Rows.Clear();

            var nGram = new NGram(2);
            NGramDistance NGramDistance = new NGramDistance();

            tSql = "select kelime from kelimeler";
            tCon.Open();
            cmd.CommandText = tSql;
            tDataReader = cmd.ExecuteReader();
            while (tDataReader.Read())
            {
                sayac++;
                prgGenel.Value = Convert.ToInt32((100 * sayac) / prgHesap);

                foreach (string eleman in aList)
                {
                    hesap = Convert.ToInt32(NGramDistance.GetDistance(eleman, tDataReader["kelime"].ToString()) * 100);
                    if (hesap > 20)
                    {
                        dgrTab7.Rows.Add(eleman, hesap, tDataReader["kelime"].ToString());
                        tToplamNgram += hesap;
                        adetNgram += 1;
                    }
                }
            }

            tDataReader.Close();
            bag_close();

            dgrTab7.Sort(dgrTab7.Columns[1], ListSortDirection.Descending);

            //lblNgram.Text = tToplamNgram / adetNgram + "%";
            //prgNgram.Value = Convert.ToInt32(tToplamNgram / Convert.ToDouble(adetNgram));
            bpgrNgram.Value = Convert.ToInt32(tToplamNgram / Convert.ToDouble(adetNgram));
            ngram_sonuc();
        }

        public void q_gram()
        {
            sayac = 0;
            kelimeleri_ayir();
            dgrTab8.Rows.Clear();

            var qGram = new QGram(2);
            tSql = "select kelime from kelimeler";
            tCon.Open();
            cmd.CommandText = tSql;
            tDataReader = cmd.ExecuteReader();
            while (tDataReader.Read())
            {
                sayac++;
                prgGenel.Value = Convert.ToInt32((100 * sayac) / prgHesap);

                foreach (string eleman in aList)
                {
                    hesap = Convert.ToInt32((100 / eleman.Length) *
                                            ((eleman.Length + tDataReader["kelime"].ToString().Length -
                                              qGram.Distance(eleman, tDataReader["kelime"].ToString())) /
                                             2));

                    if (hesap > 20)
                    {
                        dgrTab8.Rows.Add(eleman, hesap, tDataReader["kelime"].ToString());
                        tToplamQgram += hesap;
                        adetQgram += 1;
                    }
                }
            }

            tDataReader.Close();
            bag_close();

            dgrTab8.Sort(dgrTab8.Columns[1], ListSortDirection.Descending);

            //lblQgram.Text = tToplamQgram / adetQgram + "%";
            //prgQgram.Value = Convert.ToInt32(tToplamQgram / Convert.ToDouble(adetQgram));
            bpgrQgram.Value = Convert.ToInt32(tToplamQgram / Convert.ToDouble(adetQgram));
            qgram_sonuc();
        }

        public void LCS()
        {
            sayac = 0;
            kelimeleri_ayir();
            dgrTab9.Rows.Clear();

            LongestCommonSubsequence lcs = new LongestCommonSubsequence();
            tSql = "select kelime from kelimeler";
            tCon.Open();
            cmd.CommandText = tSql;
            tDataReader = cmd.ExecuteReader();
            while (tDataReader.Read())
            {
                sayac++;
                prgGenel.Value = Convert.ToInt32((100 * sayac) / prgHesap);

                foreach (string eleman in aList)
                {
                    hesap = Convert.ToDouble((lcs.GetLCS(eleman, tDataReader["kelime"].ToString())).Length);
                    hesap = Convert.ToInt32(hesap * 100 / eleman.Length);

                    if (hesap > 20)
                    {
                        dgrTab9.Rows.Add(eleman, hesap, tDataReader["kelime"].ToString());
                        tToplamLcs += hesap;
                        adetLcs += 1;
                    }
                }
            }

            tDataReader.Close();
            bag_close();

            dgrTab9.Sort(dgrTab9.Columns[1], ListSortDirection.Descending);


            bpgrLcs.Value = Convert.ToInt32(tToplamLcs / Convert.ToDouble(adetLcs));
            lcs_sonuc();
        }

        //private void btnAra_Click_1(object sender, EventArgs e)
        //{

        //    bprgHamming.Value = 0;
        //    bprgJaccard.Value = 0;
        //    bprgJaro.Value = 0;
        //    bprgJarowiki.Value = 0;
        //    bprgLevens.Value = 0;
        //    bpgrNgram.Value = 0;
        //    bpgrQgram.Value = 0;
        //    bpgrLcs.Value = 0;

        //    lblZamanHamming.Text = "";
        //    lblZamanJaccard.Text = "";
        //    lblZamanJaro.Text = "";
        //    lblZamanJaroWink.Text = "";
        //    lblZamanLevens.Text = "";
        //    lblZamanNgram.Text = "";
        //    lblZamanQgram.Text = "";
        //    lblZamanLcs.Text = "";

        //    lblDidHamming.Text = "";
        //    lblDidJaccard.Text = "";
        //    lblDidJaro.Text = "";
        //    lblDidJarowink.Text = "";
        //    lblDidLevens.Text = "";
        //    lblDidNgram.Text = "";
        //    lblDidQgram.Text = "";
        //    lblDidLcs.Text = "";

        //    Stopwatch watch1 = new Stopwatch();
        //    Stopwatch watch2 = new Stopwatch();
        //    Stopwatch watch3 = new Stopwatch();
        //    Stopwatch watch4 = new Stopwatch();
        //    Stopwatch watch5 = new Stopwatch();
        //    Stopwatch watch6 = new Stopwatch();
        //    Stopwatch watch7 = new Stopwatch();
        //    Stopwatch watch8 = new Stopwatch();

        //    uzunluk_hesabi();
        //    if (chkHamming.Value == true)
        //    {

        //        watch1.Start();
        //        hamming();
        //        watch1.Stop();
        //        lblZamanHamming.Text = "" + Convert.ToInt32(watch1.Elapsed.TotalSeconds) + " Sn";


        //    }
        //    if (chkJakkard.Value == true)
        //    {

        //        watch2.Start();
        //        Jaccard();
        //        watch2.Stop();
        //        lblZamanJaccard.Text = "" + Convert.ToInt32(watch2.Elapsed.TotalSeconds) + " Sn"; ;
        //    }
        //    if (chkJaro.Value == true)
        //    {

        //        watch3.Start();
        //        Jaro();
        //        watch3.Stop();
        //        lblZamanJaro.Text = "" + Convert.ToInt32(watch3.Elapsed.TotalSeconds) + " Sn";
        //    }
        //    if (chkJaroWink.Value == true)
        //    {

        //        watch4.Start();
        //        jaro_wink();
        //        watch4.Stop();
        //        lblZamanJaroWink.Text = "" + Convert.ToInt32(watch4.Elapsed.TotalSeconds) + " Sn";
        //    }
        //    if (chkLevens.Value == true)
        //    {

        //        watch5.Start();
        //        Levenshtein();
        //        watch5.Stop();
        //        lblZamanLevens.Text = "" + Convert.ToInt32(watch5.Elapsed.TotalSeconds) + " Sn";
        //    }
        //    if (chkNgram.Value == true)
        //    {

        //        watch6.Start();
        //        n_gram();
        //        watch6.Stop();
        //        lblZamanNgram.Text = "" + Convert.ToInt32(watch6.Elapsed.TotalSeconds) + " Sn";
        //    }
        //    if (chkQgram.Value == true)
        //    {

        //        watch7.Start();
        //        q_gram();
        //        watch7.Stop();
        //        lblZamanQgram.Text = "" + Convert.ToInt32(watch7.Elapsed.TotalSeconds) + " Sn";
        //    }
        //    if (chkLcs.Value == true)
        //    {

        //        watch8.Start();
        //        LCS();
        //        watch8.Stop();
        //        lblZamanLcs.Text = "" + Convert.ToInt32(watch8.Elapsed.TotalSeconds) + " Sn";
        //    }

        //    int toplam = Convert.ToInt32(watch1.Elapsed.TotalSeconds + watch2.Elapsed.TotalSeconds +
        //                                 watch3.Elapsed.TotalSeconds +
        //                                 watch4.Elapsed.TotalSeconds + watch5.Elapsed.TotalSeconds +
        //                                 watch6.Elapsed.TotalSeconds +
        //                                 watch7.Elapsed.TotalSeconds + watch8.Elapsed.TotalSeconds);


        //    if (watch1.Elapsed.TotalSeconds != 0)
        //    {
        //        prgHamming.Value = Convert.ToInt32(watch1.Elapsed.TotalSeconds * 100 / toplam);
        //    }
        //    else
        //    {
        //        prgHamming.Value = 0;
        //    }


        //    if (watch2.Elapsed.TotalSeconds != 0)
        //    {
        //        prgJaccard.Value = Convert.ToInt32(watch2.Elapsed.TotalSeconds * 100 / toplam);
        //    }
        //    else
        //    {
        //        prgJaccard.Value = 0;
        //    }


        //    if (watch3.Elapsed.TotalSeconds != 0)
        //    {
        //        prgJaro.Value = Convert.ToInt32(watch3.Elapsed.TotalSeconds * 100 / toplam);
        //    }
        //    else
        //    {
        //        prgJaro.Value = 0;
        //    }


        //    if (watch4.Elapsed.TotalSeconds != 0)
        //    {
        //        prgJarowiki.Value = Convert.ToInt32(watch4.Elapsed.TotalSeconds * 100 / toplam);
        //    }
        //    else
        //    {
        //        prgJarowiki.Value = 0;
        //    }

        //    if (watch5.Elapsed.TotalSeconds != 0)
        //    {
        //        prgLevens.Value = Convert.ToInt32(watch5.Elapsed.TotalSeconds * 100 / toplam);
        //    }
        //    else
        //    {
        //        prgLevens.Value = 0;
        //    }


        //    if (watch6.Elapsed.TotalSeconds != 0)
        //    {
        //        prgNgram.Value = Convert.ToInt32(watch6.Elapsed.TotalSeconds * 100 / toplam);
        //    }
        //    else
        //    {
        //        prgNgram.Value = 0;
        //    }

        //    if (watch7.Elapsed.TotalSeconds != 0)
        //    {
        //        prgQgram.Value = Convert.ToInt32(watch7.Elapsed.TotalSeconds * 100 / toplam);
        //    }
        //    else
        //    {
        //        prgQgram.Value = 0;
        //    }

        //    if (watch8.Elapsed.TotalSeconds != 0)
        //    {
        //        prgLcs.Value = Convert.ToInt32(watch8.Elapsed.TotalSeconds * 100 / toplam);
        //    }
        //    else
        //    {
        //        prgLcs.Value = 0;
        //    }
        //}

        private void bunifuTileButton1_Click(object sender, EventArgs e)
        {
            bprgHamming.Value = 0;
            bprgJaccard.Value = 0;
            bprgJaro.Value = 0;
            bprgJarowiki.Value = 0;
            bprgLevens.Value = 0;
            bpgrNgram.Value = 0;
            bpgrQgram.Value = 0;

            //prgHamming.Value = 0;
            //prgJaccard.Value = 0;
            //prgJaro.Value = 0;
            //prgJaroWink.Value = 0;
            //prgLevens.Value = 0;
            //prgNgram.Value = 0;
            //prgQgram.Value = 0;

            //lblHamming.Text = "";
            //lblJaccard.Text = "";
            //lblJaro.Text = "";
            //lblJaroWink.Text = "";
            //lblLevenshtein.Text = "";
            //lblNgram.Text = "";
            //lblQgram.Text = "";
            lblDidHamming.Text = "";
            lblZamanHamming.Text = "";
            lblZamanJaccard.Text = "";
            lblZamanJaro.Text = "";
            lblZamanJaroWink.Text = "";
            lblZamanLevens.Text = "";
            lblZamanNgram.Text = "";
            lblZamanQgram.Text = "";

            lblDidHamming.Text = "";
            lblDidJaccard.Text = "";
            lblDidJaro.Text = "";
            lblDidJarowink.Text = "";
            lblDidLevens.Text = "";
            lblDidNgram.Text = "";
            lblDidQgram.Text = "";

            Stopwatch watch1 = new Stopwatch();
            Stopwatch watch2 = new Stopwatch();
            Stopwatch watch3 = new Stopwatch();
            Stopwatch watch4 = new Stopwatch();
            Stopwatch watch5 = new Stopwatch();
            Stopwatch watch6 = new Stopwatch();
            Stopwatch watch7 = new Stopwatch();
            Stopwatch watch8 = new Stopwatch();

            uzunluk_hesabi();
            //if (chkHamming.Checked == true)
            //{
            if (chkHamming.Value == true)
            {
                watch1.Start();
                hamming();
                watch1.Stop();
                lblZamanHamming.Text = "" + Convert.ToInt32(watch1.Elapsed.TotalSeconds) + " Sn";
            }
            if (chkJakkard.Value == true)
            {
                watch2.Start();
                Jaccard();
                watch2.Stop();
                lblZamanJaccard.Text = "" + Convert.ToInt32(watch2.Elapsed.TotalSeconds) + " Sn";
                ;
            }
            if (chkJaro.Value == true)
            {
                watch3.Start();
                Jaro();
                watch3.Stop();
                lblZamanJaro.Text = "" + Convert.ToInt32(watch3.Elapsed.TotalSeconds) + " Sn";
            }
            if (chkJaroWink.Value == true)
            {
                watch4.Start();
                jaro_wink();
                watch4.Stop();
                lblZamanJaroWink.Text = "" + Convert.ToInt32(watch4.Elapsed.TotalSeconds) + " Sn";
            }
            if (chkLevens.Value == true)
            {
                watch5.Start();
                Levenshtein();
                watch5.Stop();
                lblZamanLevens.Text = "" + Convert.ToInt32(watch5.Elapsed.TotalSeconds) + " Sn";
            }
            if (chkNgram.Value == true)
            {
                watch6.Start();
                n_gram();
                watch6.Stop();
                lblZamanNgram.Text = "" + Convert.ToInt32(watch6.Elapsed.TotalSeconds) + " Sn";
            }
            if (chkQgram.Value == true)
            {
                watch7.Start();
                q_gram();
                watch7.Stop();
                lblZamanQgram.Text = "" + Convert.ToInt32(watch7.Elapsed.TotalSeconds) + " Sn";
            }
            if (chkLcs.Value == true)
            {
                watch8.Start();
                LCS();
                watch8.Stop();
                lblZamanLcs.Text = "" + Convert.ToInt32(watch8.Elapsed.TotalSeconds) + " Sn";
            }

            double toplam = Convert.ToDouble(watch1.Elapsed.TotalSeconds + watch2.Elapsed.TotalSeconds +
                                             watch3.Elapsed.TotalSeconds +
                                             watch4.Elapsed.TotalSeconds + watch5.Elapsed.TotalSeconds +
                                             watch6.Elapsed.TotalSeconds +
                                             watch7.Elapsed.TotalSeconds + watch8.Elapsed.TotalSeconds);


            if (watch1.Elapsed.TotalSeconds != 0)
            {
                prgHamming.Value = Convert.ToInt32(watch1.Elapsed.TotalSeconds * 100 / toplam);
            }
            else
            {
                prgHamming.Value = 0;
            }


            if (watch2.Elapsed.TotalSeconds != 0)
            {
                prgJaccard.Value = Convert.ToInt32(watch2.Elapsed.TotalSeconds * 100 / toplam);
            }
            else
            {
                prgJaccard.Value = 0;
            }


            if (watch3.Elapsed.TotalSeconds != 0)
            {
                prgJaro.Value = Convert.ToInt32(watch3.Elapsed.TotalSeconds * 100 / toplam);
            }
            else
            {
                prgJaro.Value = 0;
            }


            if (watch4.Elapsed.TotalSeconds != 0)
            {
                prgJarowiki.Value = Convert.ToInt32(watch4.Elapsed.TotalSeconds * 100 / toplam);
            }
            else
            {
                prgJarowiki.Value = 0;
            }

            if (watch5.Elapsed.TotalSeconds != 0)
            {
                prgLevens.Value = Convert.ToInt32(watch5.Elapsed.TotalSeconds * 100 / toplam);
            }
            else
            {
                prgLevens.Value = 0;
            }


            if (watch6.Elapsed.TotalSeconds != 0)
            {
                prgNgram.Value = Convert.ToInt32(watch6.Elapsed.TotalSeconds * 100 / toplam);
            }
            else
            {
                prgNgram.Value = 0;
            }

            if (watch7.Elapsed.TotalSeconds != 0)
            {
                prgQgram.Value = Convert.ToInt32(watch7.Elapsed.TotalSeconds * 100 / toplam);
            }
            else
            {
                prgQgram.Value = 0;
            }

            if (watch8.Elapsed.TotalSeconds != 0)
            {
                prgLcs.Value = Convert.ToInt32(watch8.Elapsed.TotalSeconds * 100 / toplam);
            }
            else
            {
                prgLcs.Value = 0;
            }
        }


        private void bunifuCheckbox1_OnChange(object sender, EventArgs e)
        {
            if (chkGenel.Checked == true)
            {
                chkHamming.Value = true;
                chkJakkard.Value = true;
                chkJaro.Value = true;
                chkJaroWink.Value = true;
                chkLevens.Value = true;
                chkNgram.Value = true;
                chkQgram.Value = true;
                chkLcs.Value = true;
            }
            else
            {
                chkHamming.Value = false;
                chkJakkard.Value = false;
                chkJaro.Value = false;
                chkJaroWink.Value = false;
                chkLevens.Value = false;
                chkNgram.Value = false;
                chkQgram.Value = false;
                chkLcs.Value = false;
            }
        }

        private void label9_Click(object sender, EventArgs e)
        {
        }

        private void label22_Click(object sender, EventArgs e)
        {
        }

        private void lblZamanJaroWink_Click(object sender, EventArgs e)
        {
        }

        public void kelimeleri_ayir()
        {
            aList.Clear();
            int basla = 0;
            string arananCumle = txtKelime.Text.Replace('?', ' ').Replace('!', ' ').Replace('*', ' ').Replace('-', ' ')
                .Replace('/', ' ').Replace('&', ' ').Replace('.', ' ').Replace('\n', ' ');


            if (arananCumle.Length != 0)
            {
                for (int i = 0; i < arananCumle.Length; i++)
                {
                    if (arananCumle.Substring(i, 1) == " ")
                    {
                        aList.Add(arananCumle.Substring(basla, i - basla));
                        basla = i + 1;
                    }

                    if (i == arananCumle.Length - 1)
                    {
                        aList.Add(arananCumle.Substring(basla, i - basla + 1));
                    }
                }
            }
        }


        private void btnGozat_Click(object sender, EventArgs e)
        {
            //Dosyayı seç
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Documents (*.txt, *.pdf, *.docx)|*.txt; *.docx; *.pdf;";
            openDialog.Title = "Select Document";

            if (openDialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            string ext = Path.GetExtension(openDialog.FileName);


            if (ext == ".txt") //Eğer kullanıcı txt uzantılı bir dosya seçmiş ise
            {
                FileStream fStr;
                Encoding objEncoding = Encoding.Default;
                try
                {
                    fStr = new FileStream(openDialog.FileName, FileMode.Open, FileAccess.Read);
                    StreamReader sr = new StreamReader(fStr, objEncoding);
                    txtKelime.Text = sr.ReadToEnd();
                    sr.Close();
                }
                catch (Exception exception)
                {
                    MessageBox.Show("Error opening file", exception.ToString());
                }
            }
            else if (ext == ".docx") //Eğer kullanıcı docx uzantılı bir dosya seçmiş ise
            {
                try
                {
                    TextExtractor extractor = new TextExtractor(openDialog.FileName);
                    txtKelime.Text = extractor.ExtractText().Replace("\n", " ").Replace("	", " ").Replace("  ", " ");
                }
                catch (Exception exception)
                {
                    MessageBox.Show("Error opening file", exception.ToString());
                }
            }
            else if (ext == ".pdf") //Eğer kullanıcı pdf uzantılı bir dosya seçmiş ise
            {
                try
                {
                    PdfOku pdfOku = new PdfOku();
                    txtKelime.Text = pdfOku.getPdfResult(openDialog.FileName);
                }
                catch (Exception exception)
                {
                    MessageBox.Show("Error opening file", exception.ToString());
                }
            }
            else //Eğer kullanıcı farklı bir dosya seçmişse(ki mümkün değil)
            {
                MessageBox.Show("Geçerli dosya seçiniz");
            }
        }

        public void hamming_sonuc()
        {
            lblDidHamming.Text = "";
            bool durum = false;
            foreach (string eleman in aList)
            {
                for (int i = 0; i < dgrTab1.RowCount - 1; i++)
                {
                    if (dgrTab1.Rows[i].Cells["kelime1"].Value.ToString() == eleman)
                    {
                        durum = true;

                        //lblDidHamming.Text
                        lblDidHamming.Text += " " + dgrTab1.Rows[i].Cells["kelime2"].Value.ToString();
                        i = dgrTab1.RowCount;
                    }
                    if (durum == false)
                    {
                        if (i == dgrTab1.RowCount)
                        {
                            lblDidHamming.Text += " " + eleman;
                        }
                    }
                }
            }
        }

        public void jaccard_sonuc()
        {
            lblDidJaccard.Text = "";
            bool durum = false;
            foreach (string eleman in aList)
            {
                for (int i = 0; i < dgrTab2.RowCount; i++)
                {
                    if (dgrTab2.Rows[i].Cells["jaccardkelime1"].Value.ToString() == eleman)
                    {
                        durum = true;
                        lblDidJaccard.Text += " " + dgrTab2.Rows[i].Cells["jaccardkelime2"].Value.ToString();
                        i = dgrTab2.RowCount;
                    }
                    if (durum == false)
                    {
                        if (i == dgrTab2.RowCount)
                        {
                            lblDidJaccard.Text += " " + eleman;
                        }
                    }
                }
            }
        }

        public void jaro_sonuc()
        {
            lblDidJaro.Text = "";
            bool durum = false;
            foreach (string eleman in aList)
            {
                for (int i = 0; i < dgrTab4.RowCount; i++)
                {
                    if (dgrTab4.Rows[i].Cells["jarokelime1"].Value.ToString() == eleman)
                    {
                        durum = true;
                        lblDidJaro.Text += " " + dgrTab4.Rows[i].Cells["jarokelime2"].Value.ToString();
                        i = dgrTab4.RowCount;
                    }
                    if (durum == false)
                    {
                        if (i == dgrTab4.RowCount)
                        {
                            lblDidJaro.Text += " " + eleman;
                        }
                    }
                }
            }
        }


        public void jarowink_sonuc()
        {
            lblDidJarowink.Text = "";
            bool durum = false;
            foreach (string eleman in aList)
            {
                for (int i = 0; i < dgrTab5.RowCount; i++)
                {
                    if (dgrTab5.Rows[i].Cells["jarowinkkelime1"].Value.ToString() == eleman)
                    {
                        durum = true;
                        lblDidJarowink.Text += " " + dgrTab5.Rows[i].Cells["jarowinkkelime2"].Value.ToString();
                        i = dgrTab5.RowCount;
                    }
                    if (durum == false)
                    {
                        if (i == dgrTab5.RowCount)
                        {
                            lblDidJarowink.Text += " " + eleman;
                        }
                    }
                }
            }
        }

        public void levens_sonuc()
        {
            lblDidLevens.Text = "";
            bool durum = false;
            foreach (string eleman in aList)
            {
                for (int i = 0; i < dgrTab6.RowCount; i++)
                {
                    if (dgrTab6.Rows[i].Cells["levenskelime1"].Value.ToString() == eleman)
                    {
                        durum = true;
                        lblDidLevens.Text += " " + dgrTab6.Rows[i].Cells["levenskelime2"].Value.ToString();
                        i = dgrTab6.RowCount;
                    }
                    if (durum == false)
                    {
                        if (i == dgrTab6.RowCount)
                        {
                            lblDidLevens.Text += " " + eleman;
                        }
                    }
                }
            }
        }

        public void ngram_sonuc()
        {
            lblDidNgram.Text = "";
            bool durum = false;
            foreach (string eleman in aList)
            {
                for (int i = 0; i < dgrTab7.RowCount; i++)
                {
                    if (dgrTab7.Rows[i].Cells["ngramkelime1"].Value.ToString() == eleman)
                    {
                        durum = true;
                        lblDidNgram.Text += " " + dgrTab7.Rows[i].Cells["ngramkelime2"].Value.ToString();
                        i = dgrTab7.RowCount;
                    }
                    if (durum == false)
                    {
                        if (i == dgrTab7.RowCount)
                        {
                            lblDidNgram.Text += " " + eleman;
                        }
                    }
                }
            }
        }

        public void qgram_sonuc()
        {
            lblDidQgram.Text = "";
            bool durum = false;
            foreach (string eleman in aList)
            {
                for (int i = 0; i < dgrTab8.RowCount; i++)
                {
                    if (dgrTab8.Rows[i].Cells["qgramkelime1"].Value.ToString() == eleman)
                    {
                        durum = true;
                        lblDidQgram.Text += " " + dgrTab8.Rows[i].Cells["qgramkelime2"].Value.ToString();
                        i = dgrTab8.RowCount;
                    }
                    if (durum == false)
                    {
                        if (i == dgrTab8.RowCount)
                        {
                            lblDidQgram.Text += " " + eleman;
                        }
                    }
                }
            }
        }

        public void lcs_sonuc()
        {
            lblDidLcs.Text = "";
            bool durum = false;
            foreach (string eleman in aList)
            {
                for (int i = 0; i < dgrTab9.RowCount; i++)
                {
                    if (dgrTab9.Rows[i].Cells["lcsKelime1"].Value.ToString() == eleman)
                    {
                        durum = true;
                        lblDidLcs.Text += " " + dgrTab9.Rows[i].Cells["lcsKelime2"].Value.ToString();
                        i = dgrTab9.RowCount;
                    }
                    if (durum == false)
                    {
                        if (i == dgrTab9.RowCount)
                        {
                            lblDidLcs.Text += " " + eleman;
                        }
                    }
                }
            }
        }


        private void btnAra_Click(object sender, EventArgs e)
        {
            bprgHamming.Value = 0;
            bprgJaccard.Value = 0;
            bprgJaro.Value = 0;
            bprgJarowiki.Value = 0;
            bprgLevens.Value = 0;
            bpgrNgram.Value = 0;
            bpgrQgram.Value = 0;

            //prgHamming.Value = 0;
            //prgJaccard.Value = 0;
            //prgJaro.Value = 0;
            //prgJaroWink.Value = 0;
            //prgLevens.Value = 0;
            //prgNgram.Value = 0;
            //prgQgram.Value = 0;

            //lblHamming.Text = "";
            //lblJaccard.Text = "";
            //lblJaro.Text = "";
            //lblJaroWink.Text = "";
            //lblLevenshtein.Text = "";
            //lblNgram.Text = "";
            //lblQgram.Text = "";

            lblZamanHamming.Text = "";
            lblZamanJaccard.Text = "";
            lblZamanJaro.Text = "";
            lblZamanJaroWink.Text = "";
            lblZamanLevens.Text = "";
            lblZamanNgram.Text = "";
            lblZamanQgram.Text = "";

            lblDidHamming.Text = "";
            lblDidJaccard.Text = "";
            lblDidJaro.Text = "";
            lblDidJarowink.Text = "";
            lblDidLevens.Text = "";
            lblDidNgram.Text = "";
            lblDidQgram.Text = "";

            Stopwatch watch1 = new Stopwatch();
            Stopwatch watch2 = new Stopwatch();
            Stopwatch watch3 = new Stopwatch();
            Stopwatch watch4 = new Stopwatch();
            Stopwatch watch5 = new Stopwatch();
            Stopwatch watch6 = new Stopwatch();
            Stopwatch watch7 = new Stopwatch();

            uzunluk_hesabi();
            if (chkHamming.Value == true)
            {
                watch1.Start();
                hamming();
                watch1.Stop();
                lblZamanHamming.Text = "" + Convert.ToInt32(watch1.Elapsed.TotalSeconds) + " Sn";
            }
            if (chkJakkard.Value == true)
            {
                watch2.Start();
                Jaccard();
                watch2.Stop();
                lblZamanJaccard.Text = "" + Convert.ToInt32(watch2.Elapsed.TotalSeconds) + " Sn";
                ;
            }
            if (chkJaro.Value == true)
            {
                watch3.Start();
                Jaro();
                watch3.Stop();
                lblZamanJaro.Text = "" + Convert.ToInt32(watch3.Elapsed.TotalSeconds) + " Sn";
            }
            if (chkJaroWink.Value == true)
            {
                watch4.Start();
                jaro_wink();
                watch4.Stop();
                lblZamanJaroWink.Text = "" + Convert.ToInt32(watch4.Elapsed.TotalSeconds) + " Sn";
            }
            if (chkLevens.Value == true)
            {
                watch5.Start();
                Levenshtein();
                watch5.Stop();
                lblZamanLevens.Text = "" + Convert.ToInt32(watch5.Elapsed.TotalSeconds) + " Sn";
            }
            if (chkNgram.Value == true)
            {
                watch6.Start();
                n_gram();
                watch6.Stop();
                lblZamanNgram.Text = "" + Convert.ToInt32(watch6.Elapsed.TotalSeconds) + " Sn";
            }
            if (chkQgram.Value == true)
            {
                watch7.Start();
                q_gram();
                watch7.Stop();
                lblZamanQgram.Text = "" + Convert.ToInt32(watch7.Elapsed.TotalSeconds) + " Sn";
            }

            int toplam = Convert.ToInt32(watch1.Elapsed.TotalSeconds + watch2.Elapsed.TotalSeconds +
                                         watch3.Elapsed.TotalSeconds +
                                         watch4.Elapsed.TotalSeconds + watch5.Elapsed.TotalSeconds +
                                         watch6.Elapsed.TotalSeconds +
                                         watch7.Elapsed.TotalSeconds);


            if (watch1.Elapsed.TotalSeconds != 0)
            {
                prgHamming.Value = Convert.ToInt32(watch1.Elapsed.TotalSeconds * 100 / toplam);
            }
            else
            {
                prgHamming.Value = 0;
            }


            if (watch2.Elapsed.TotalSeconds != 0)
            {
                prgJaccard.Value = Convert.ToInt32(watch2.Elapsed.TotalSeconds * 100 / toplam);
            }
            else
            {
                prgJaccard.Value = 0;
            }


            if (watch3.Elapsed.TotalSeconds != 0)
            {
                prgJaro.Value = Convert.ToInt32(watch3.Elapsed.TotalSeconds * 100 / toplam);
            }
            else
            {
                prgJaro.Value = 0;
            }


            if (watch4.Elapsed.TotalSeconds != 0)
            {
                prgJarowiki.Value = Convert.ToInt32(watch4.Elapsed.TotalSeconds * 100 / toplam);
            }
            else
            {
                prgJarowiki.Value = 0;
            }

            if (watch5.Elapsed.TotalSeconds != 0)
            {
                prgLevens.Value = Convert.ToInt32(watch5.Elapsed.TotalSeconds * 100 / toplam);
            }
            else
            {
                prgLevens.Value = 0;
            }


            if (watch6.Elapsed.TotalSeconds != 0)
            {
                prgNgram.Value = Convert.ToInt32(watch6.Elapsed.TotalSeconds * 100 / toplam);
            }
            else
            {
                prgNgram.Value = 0;
            }

            if (watch7.Elapsed.TotalSeconds != 0)
            {
                prgQgram.Value = Convert.ToInt32(watch7.Elapsed.TotalSeconds * 100 / toplam);
            }
            else
            {
                prgQgram.Value = 0;
            }
        }


        public void bag_close()
        {
            tCon.Close();
            Refresh();
        }

        public static int GetHammingDistance(string s, string t)
        {
            if (s.Length != t.Length)
            {
                MessageBox.Show("Uzunluklar Eşit olmalı");
            }

            int distance =
                s.ToCharArray()
                    .Zip(t.ToCharArray(), (c1, c2) => new {c1, c2})
                    .Count(m => m.c1 != m.c2);

            return distance;
        }
        //  Hamming ----------------------------------------------------------------

        //  Jaccard   ----------------------------------------------------------------
        public static double Calc(HashSet<string> hs1, HashSet<string> hs2)
        {
            return ((double) hs1.Intersect(hs2).Count());
        }

        public static double Calc(List<string> ls1, List<string> ls2)
        {
            HashSet<string> hs1 = new HashSet<string>(ls1);
            HashSet<string> hs2 = new HashSet<string>(ls2);
            return Calc(hs1, hs2);
        }

        public double JaccardTest1(string kelime1, string kelime2)
        {
            List<string> docK1 = new List<string>();

            for (int i = 0; i < kelime1.Length - 1; i++)
            {
                docK1.Add(kelime1.Substring(i, 2));
            }

            List<string> docK2 = new List<string>();
            for (int j = 0; j < kelime2.Length - 1; j++)
            {
                docK2.Add(kelime2.Substring(j, 2));
            }

            return Calc(docK1, docK2);
        }


        //  Jaccard   ----------------------------------------------------------------

        //  Euclidean   ----------------------------------------------------------------
        private static int CalcLevenshteinDistance(string a, string b)
        {
            if (String.IsNullOrEmpty(a) || String.IsNullOrEmpty(b)) return 0;

            int lengthA = a.Length;
            int lengthB = b.Length;
            var distances = new int[lengthA + 1, lengthB + 1];
            for (int i = 0; i <= lengthA; distances[i, 0] = i++) ;
            for (int j = 0; j <= lengthB; distances[0, j] = j++) ;

            for (int i = 1; i <= lengthA; i++)
            for (int j = 1; j <= lengthB; j++)
            {
                int cost = b[j - 1] == a[i - 1] ? 0 : 1;
                distances[i, j] = Math.Min
                (
                    Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
                    distances[i - 1, j - 1] + cost
                );
            }
            return distances[lengthA, lengthB];
        }

        //  Euclidean   ----------------------------------------------------------------

        private void txtKelime_TextChanged(object sender, EventArgs e)
        {
        }

        public static double JaroDistance(string source, string target)
        {
            int m = source.Intersect(target).Count();

            if (m == 0)
            {
                return 0;
            }
            else
            {
                string sourceTargetIntersetAsString = "";
                string targetSourceIntersetAsString = "";
                IEnumerable<char> sourceIntersectTarget = source.Intersect(target);
                IEnumerable<char> targetIntersectSource = target.Intersect(source);
                foreach (char character in sourceIntersectTarget)
                {
                    sourceTargetIntersetAsString += character;
                }
                foreach (char character in targetIntersectSource)
                {
                    targetSourceIntersetAsString += character;
                }
                double t = sourceTargetIntersetAsString.LevenshteinDistance(targetSourceIntersetAsString) / 2;
                return ((m / source.Length) + (m / target.Length) + ((m - t) / m)) / 3;
            }
        }

        private void dgrTab5_ColumnAdded(object sender, DataGridViewColumnEventArgs e)
        {
        }
    }
}