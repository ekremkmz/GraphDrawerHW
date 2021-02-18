using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Ayrik_Proje_Gui
{
    public partial class Form2 : Form
    {
        String path;
        List<Node> dersListesi = new List<Node>();
        static List<List<Node>> graflar = new List<List<Node>>();
        int renkSayisi;
        public Form2(String path)
        {
            InitializeComponent();
            this.path = path;
            Calculate();
        }
        public void Calculate()
        {
            String[] dersler = File.ReadAllLines(path + @"\Dersler.txt");//txt dosyasinda bulunan ders isimleri diziye cekiliyor
            foreach (String dersAdi in dersler)//Dizide bulunan ders isimleri listedeki dugumlere aktariliyior
            {
                dersListesi.Add(new Node(dersAdi));
            }
            foreach (Node ders in dersListesi)//Derslistesinde geziliyor
            {
                String[] ogrenciler = File.ReadAllLines(path + @"\" + ders.getName() + ".txt");//Dersi alan ogrenciler diziye cekiliyor
                foreach (String s in ogrenciler)
                {
                    ders.getAlanOgrenciler().Add(s);//Dersi alan ogrenciler dugumdeki listeye ekleniyor
                }
            }
            komsulariEsle(dersListesi);
            renkSayisi = markAll(dersListesi);
        }
        static int markAll(List<Node> dugumler)
        {
            int renkSayisi = -1,tmp = -1;
            foreach (Node item in dugumler)
            {
                //Isaretlenmemis dugum kalmayana kadar isaretleme sureci baslatir. Kopukluk olan graflarda da calismis olur
                if (item.getMark() == -1)//Kopma durumu varsa burasi birden fazla kez calisacaktir.Bu noktada da graflara boluncek
                {
                    tmp++;
                    graflar.Add(new List<Node>());
                    mark(item, 0, graflar[tmp]);
                }
                renkSayisi = (renkSayisi < item.getMark()) ? item.getMark() : renkSayisi;//En buyuk marker degeri bulunur
            }
            return renkSayisi + 1;
        }
        static void mark(Node dugum, int marker,List<Node> graf)
        {
            if (dugum.getMark() == -1)//Fonksiyon icinde tekrar kontrol etme sebebim recursive fonksiyonun sonlanabilmesi icin
            {
                while (checkKomsular(dugum, marker))//Marker degeri komsularda var mi kontrol ediliyor
                {
                    marker++;
                }
                dugum.setMark(marker);
                graf.Add(dugum);//Yeni boyandigi zaman graf'a eklenecek
                foreach (Node item in dugum.getKomsular())
                {
                    mark(item, 0,graf);
                }
            }
        }
        static bool checkKomsular(Node dugum, int marker)
        {
            foreach (Node komsu in dugum.getKomsular())
            {
                if (komsu.getMark() == marker)
                {
                    return true;
                }
            }
            return false;
        }
        static void komsulariEsle(List<Node> dersListesi)
        {
            for (int i = 0; i < dersListesi.Count; i++)//Dersler arasindaki tum ikili kombinasyonlari gezer
            {
                for (int j = i + 1; j < dersListesi.Count; j++)
                {
                    if (searchStudentConflict(dersListesi[i], dersListesi[j]))
                    {//Ogrenci cakismasi varsa birbirine komsu olarak eklenir
                        dersListesi[i].getKomsular().Add(dersListesi[j]);
                        dersListesi[j].getKomsular().Add(dersListesi[i]);
                    }
                }
            }
        }
        static bool searchStudentConflict(Node ders1, Node ders2)
        {
            foreach (String ogr1 in ders1.getAlanOgrenciler())
            {
                foreach (String ogr2 in ders2.getAlanOgrenciler())
                {
                    if (String.Compare(ogr1, ogr2) == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private void Form2_Load(object sender, EventArgs e)
        {
            
        }

        private void Form2_Paint(object sender, PaintEventArgs e)
        {

            int konumMerkeziX = 120,konumMerkeziY=120;
            int cap = 100;
            int textCoordx = 50,textCoordY = 250;
            Graphics a = e.Graphics;
            Pen p = new Pen(Color.Black, 2);
            Font f = new Font("Arial", 14);
            SolidBrush s = new SolidBrush(Color.Black);
            foreach (List<Node> graf in graflar)
            {
                for (int i = 0; i < graf.Count; i++)
                {
                    graf[i].coordX = (float)konumMerkeziX + cap * (float)Math.Cos(i * 2 * Math.PI / graf.Count);
                    graf[i].coordY = (float)konumMerkeziY + cap * (float)Math.Sin(i * 2 * Math.PI / graf.Count);
                }
                konumMerkeziX += 250;
            }
            foreach (List<Node> graf in graflar)
            {
                foreach (Node dugum1 in graf)
                {
                    foreach (Node dugum2 in dugum1.getKomsular())
                    {
                        a.DrawLine(p, dugum1.coordX+15, dugum1.coordY+15, dugum2.coordX+15, dugum2.coordY+15);
                    }
                }
            }
            foreach (List<Node> graf in graflar)
            {
                foreach (Node dugum in graf)
                {
                    a.FillEllipse((renkSayisi > 6) ? new SolidBrush(Color.FromArgb(((dugum.getMark() + 1) / 9 * 255 / 3), (((dugum.getMark() + 1) / 3) % 3 + 1) * 255 / 3, ((dugum.getMark() + 1) % 3 + 1) * 255 / 3)) : 
                        new SolidBrush(Color.FromArgb(((dugum.getMark() + 1) / 4 * 255), (((dugum.getMark() + 1) / 2) % 2) * 255, ((dugum.getMark() + 1) % 2) * 255)), dugum.coordX, dugum.coordY, 30, 30) ;
                    a.DrawString(dugum.nickName, f, s , dugum.coordX + 5 , dugum.coordY + 5);
                }
            }
            foreach(Node dugum in dersListesi)
            {
                a.DrawString(dugum.nickName + " --> " + dugum.getName(), f, s, textCoordx, textCoordY);
                textCoordY += 20;
            }
            a.DrawString("Minimum gerekli renk sayısı : " + renkSayisi.ToString(), f, s, textCoordx, textCoordY);
        }
    }
    class Node
    {
        String name;
        public String nickName { get; set; }
        static char newNick = 'A';
        List<String> alanOgrenciler = new List<string>();
        List<Node> komsular = new List<Node>();
        int mark = -1;//Isaretsiz olan dugumleri belirtir
        public float coordX { get; set; }
        public float coordY { get; set; }
        public Node(String name)
        {
            this.name = name;
            this.nickName = newNick.ToString();
            newNick++;
        }
        public String getName()
        {
            return name;
        }
        public List<Node> getKomsular()
        {
            return komsular;
        }
        public List<String> getAlanOgrenciler()
        {
            return alanOgrenciler;
        }
        public int getMark()
        {
            return mark;
        }
        public void setMark(int mark)
        {
            this.mark = mark;
        }
    }
}
