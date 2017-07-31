using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace jatek2
{
    class BusinessLogic:Bindable
    {

        ViewModel vm;
        DispatcherTimer dt;
        DispatcherTimer kutyatimer;

        int[,] map;
        public int[,] Map { get { return map; } set { map = value; OPC(); } }

        Point vakond;
        public Point Vakond { get { return vakond; } set { vakond = value; OPC(); } }

        Point kutya;
        public Point Kutya { get { return kutya; } set { kutya = value; OPC(); } }

        int gilisztakszama;
        public int GilisztakSzama { get { return gilisztakszama; } set { gilisztakszama = value; OPC(); } }

        bool vege;

        int elet=3;
        public int Elet { get { return elet; } set { elet = value; OPC(); } }

        int ido=60;

        int dx;

        string maradekido;
        public string MaradekIdo { get { return maradekido; } set { maradekido = value; OPC(); } }

        int palya = 1;

        public BusinessLogic(ViewModel vm)
        {
            this.vm = vm;
            dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromMilliseconds(1000);
            dt.Tick += Dt_Tick;

            dx = 1;
            Elet = 3;
            kutyatimer = new DispatcherTimer();
            kutyatimer.Interval = TimeSpan.FromMilliseconds(16);
            kutyatimer.Tick += Kutyatimer_Tick;  

            PalyaBetolt(palya, 0);
            Pause();
        }

        public void Pause()
        {
            kutyatimer.Stop();
            vm.IsInMenu = true;
            dt.Stop();   
        }
        public void UnPause()
        {
            kutyatimer.Start();
            vm.IsInLevelChooser = false;
            vm.IsInHelp = false;
            vm.IsInMenu = false;
            dt.Start();
        }
        private void Kutyatimer_Tick(object sender, EventArgs e)
        {
            int x = (int)Kutya.X;
            int y = (int)Kutya.Y;

            if (x + dx < 0 || x + dx == Map.GetLength(1))
                dx *= -1;
            Map[(int)Kutya.Y, (int)Kutya.X] = 1;
            Kutya = new Point(x + dx, y);
            Map[(int)Kutya.Y, (int)Kutya.X] = 9;
            OPC("Map");
            Utkozik();
        }

        private void Dt_Tick(object sender, EventArgs e)
        {
            ido--;
            MaradekIdo = Atvaltas(ido);
            if (ido == 0)
            {
                VegeVan();
            }
        }

        string Atvaltas(int szam)
        {
            return (szam/60+":"+szam%60);
        }

        public void PalyaBetolt(int idx,int ujido)
        {
            dt.Start();
            Kutya = new Point(0,0);
            kutyatimer.Start();
            string[] sorok = File.ReadAllLines("Level" + idx + ".txt");
            int w = int.Parse(sorok[0]);
            int h = int.Parse(sorok[1]);
            GilisztakSzama = int.Parse(sorok[2]);
            int[,] m = new int[h, w];
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    switch(sorok[3+i][j])
                    {
                        case 'f': m[i, j] = 0; break; //föld
                        case 'l': m[i, j] = 1; break; //levegő
                        case 'a': m[i, j] = 2; break; //állapotsor
                        case 'g': m[i, j] = 3; break; //giliszta
                        case 'V': Vakond=new Point(j,i); m[i, j] = 5; break; //vakond
                        case 'x': m[i, j] = 6; break; //víz
                        case 'h': m[i, j] = 7; break; //állapotsorban giliszta
                        case 's': m[i, j] = 8; break; //állapotsorban életerő
                    }
                }
            }
            m[0,0]=9;
            Map = m;
            vege = false;
            ido = ujido;
            this.palya = idx;
        }
        public void Mozog(int dx, int dy)
        {
            if (vege)
                return;
            if (Vakond.X + dx >= 0 && Vakond.Y + dy >= 0 && Vakond.X + dx < Map.GetLength(1) && Vakond.Y + dy < Map.GetLength(0)-1)
            {
                Map[(int)Vakond.Y, (int)Vakond.X] = 4;
                Vakond = new Point(Vakond.X + dx, Vakond.Y + dy);
                GilisztatTalalt();
                VizbeLep();
            }
        }
        void GilisztatTalalt()
        {
            if (Map[(int)Vakond.Y, (int)Vakond.X] == 3)
            {
                GilisztakSzama--;
                OPC("GilisztakSzama");
            }
            Map[(int)Vakond.Y, (int)Vakond.X] = 5;
            OPC("Map");
            if (GilisztakSzama==0)
            {
                dt.Stop();
                kutyatimer.Stop();
                MessageBoxResult msr= MessageBox.Show("Gratulálok! Sikerült összeszedni az összes gilisztát! Szeretnél a következő pályára menni?","",MessageBoxButton.YesNo);
                if (msr == MessageBoxResult.Yes && palya<4)
                {
                    int ido = 30;
                    if (palya + 1 < 5)
                    {
                        palya++;
                        switch (palya)
                        {
                            case 2: ido = 40; break;
                            case 3: ido = 50; break;
                            case 4: ido = 60; break;
                        }
                        PalyaBetolt(palya, ido);
                    }
                }
                else
                {
                    vm.IsInLevelChooser = true;
                }
            }
        }
        bool VizbeLep()
        {
            int i = (int)Vakond.Y;
            int j = (int)Vakond.X;

            if (j == 0 && i > 0) //ha a bal szélén van az ablaknak
                while (i < Map.GetLength(0) - 1 && j == 0)
                {
                    if (Map[(int)Vakond.Y + 1, (int)Vakond.X] == 6 || Map[(int)Vakond.Y - 1, (int)Vakond.X] == 6)
                    {                
                        VegeVan();
                        return true;                    
                    }
                    j++;
                }

            i = (int)Vakond.Y;
            j = (int)Vakond.X;

            if (j == map.GetLength(1)-1 && i > 0) //ha a jobb szélén van az ablaknak
                while (i < Map.GetLength(0) - 1 && j == map.GetLength(1) - 1)
                {
                    if (Map[(int)Vakond.Y + 1, (int)Vakond.X] == 6 || Map[(int)Vakond.Y - 1, (int)Vakond.X] == 6)
                    {
                        VegeVan();
                        return true;
                    }
                    j++;
                }

            i = (int)Vakond.Y;
            j = (int)Vakond.X;

            if (i>0 && j>0)
            while (i < Map.GetLength(0) - 1 && j < map.GetLength(1) - 1 && i > 0 && j > 0)
            {
                if (Map[(int)Vakond.Y + 1, (int)Vakond.X] == 6 || Map[(int)Vakond.Y, (int)Vakond.X + 1] == 6 || Map[(int)Vakond.Y - 1, (int)Vakond.X] == 6 || Map[(int)Vakond.Y, (int)Vakond.X - 1] == 6)
                {
                    VegeVan();                 
                    return true;
                }
                i++;
                j++;
            }
            return false;
        }
        void Utkozik()
        {
            if (Kutya.X==Vakond.X && Kutya.Y==Vakond.Y)
            {
                VegeVan();
            }
        }
        void VegeVan()
        {
            Elet--;
            OPC("Elet");
            if (Elet > 0 && ido>0)
            {
                kutyatimer.Stop();
                dt.Stop();
                MessageBox.Show("Vesztettél egy életet! Vigyázz, már csak "+Elet+" életed maradt.","Hoppá!",MessageBoxButton.OK);
                PalyaBetolt(palya, ido);
            }
            if (Elet==0 && ido>0)
            {
                kutyatimer.Stop();
                dt.Stop();
                vege = true;
                MessageBox.Show("Elfogyott az összes életed! Vége a játéknak.",":(",MessageBoxButton.OK);
                Elet = 3;
                vm.IsInLevelChooser = true;
            }
            if (ido==0)
            {
                kutyatimer.Stop();
                dt.Stop();
                vege = true;
                MessageBox.Show("Lejárt az időd! Vége a játéknak.", ":(", MessageBoxButton.OK);
                Elet = 3;
                vm.IsInLevelChooser = true;
            }
        }
    }
    class ViewModel:Bindable
    {
        bool isInMenu = true;
        bool isInLevelChooser, isInHelp;
        BusinessLogic bl;
        public BusinessLogic BL { get { return bl; } set { bl = value; } }

        public bool IsInMenu { get { return isInMenu; } set { isInMenu = value; OPC(); } }

        public bool IsInLevelChooser { get { return isInLevelChooser; } set { isInLevelChooser = value; OPC(); } }

        public bool IsInHelp { get { return isInHelp; } set { isInHelp = value; OPC(); } }

        public ViewModel()
        {
            BL = new BusinessLogic(this);
        }
    }
    abstract class Bindable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OPC([CallerMemberName] string n="")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(n));
        }
    }
    class MapToGeometryConverter : IValueConverter
    {
        const int TILE = 50;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int[,] tomb = (int[,])value;
            DrawingImage di = new DrawingImage();
            DrawingGroup dg = new DrawingGroup();
            VizBetolt(tomb, dg);
            for (int i = 0; i < tomb.GetLength(0); i++)
            {
                for (int j = 0; j < tomb.GetLength(1); j++)
                {
                    Brush szin;
                    switch (tomb[i, j])
                    {
                        case 0: szin = Betolt("fold"); break;
                        case 1: szin = Betolt("fu"); break;
                        case 2: szin = Brushes.White; break;
                        case 3: szin = Betolt("giliszta2"); break;
                        case 5:
                            if (i == 0)
                                szin = Betolt("vakond2");
                            else
                                szin = Betolt("vakond");
                            break;
                        case 7: szin = Betolt("giliszta"); break;
                        case 8: szin = Betolt("sziv"); break;
                        case 9: szin = Betolt("kutya"); break;
                        default:
                            if (i != 0)
                                szin = Brushes.Transparent;
                            else
                                szin=Betolt("fu");
                            break; //kiásott föld
                    }
                    if (tomb[i,j]!=6)
                        dg.Children.Add(new GeometryDrawing(szin, null, new RectangleGeometry(new Rect(j * TILE, i * TILE, TILE, TILE))));
                }
            }
            di.Drawing = dg;
            return di;
        }

        public ImageBrush Betolt(string n) //elemek betöltése
        {
            ImageBrush ib;
            ib = new ImageBrush(new BitmapImage(new Uri(n+".png",UriKind.Relative)));
                ib.TileMode = TileMode.Tile;
                ib.Viewport = new Rect(0, 0, TILE, TILE);
                ib.ViewportUnits = BrushMappingMode.Absolute;
            return ib;
        }

        public void VizBetolt(int[,] map, DrawingGroup dg) //vizek betöltése
        {
            int m = 0;
            for (int i = 0; i < map.GetLength(0); i++)
                for (int j = 0; j < map.GetLength(1); j++)
                    if (map[i,j] == 6)
                        m++;

            List<Point> lista=new List<Point>();
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    int meretj = 1;
                    int mereti = 1;
                    if (map[i,j]==6)
                    {
                        int z = i;
                        int y = j;
                        Point pont = new Point(j,i);
                        if (!lista.Contains(pont))
                        {

                            while (map[z + 1, y] == 6)
                            {
                                mereti++;
                                z++;
                            }
                            while (map[z, y + 1] == 6 && y+1<=map.GetLength(1)-1)
                            {
                                meretj++;
                                if (y + 1 != map.GetLength(1) - 1)
                                    y++;
                                else //ha az ablak jobb szélén van a víz
                                    break; 

                            }
                            for (int l = i; l <= i+mereti; l++)
                            {
                                for (int n = j; n <= j+meretj; n++)
                                {
                                    lista.Add(new Point(n,l));
                                }
                            }
                            Brush szin=new ImageBrush(new BitmapImage(new Uri("viz.png", UriKind.Relative)));
                            dg.Children.Add(new GeometryDrawing(szin, null, new RectangleGeometry(new Rect(j * TILE, i * TILE, meretj*TILE, mereti*TILE))));

                            j = j + meretj;
                        }
                        else
                            j++;
                    }
                }
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
