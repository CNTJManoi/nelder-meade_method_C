﻿using OxyPlot;
using OxyPlot.Series;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Linq;
using Vector = nelder_meade_method.Models.Vector;

namespace nelder_meade_method.ViewModels
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        private PlotModel _plotModel;
        private List<DataPoint> _points;
        private string _function;
        private string _outputResultFunction;
        private string _outputPoint;
        public PlotModel PlotModel
        {
            get { return _plotModel; }
            set { _plotModel = value; OnPropertyChanged("PlotModel"); }
        }
        public string Function
        {
            get { return _function; }
            set
            {
                if (!string.Equals(_function, value))
                {
                    _function = value;
                    OnPropertyChanged("Function");
                }
            }
        }
        public string OutputResultFunction
        {
            get { return _outputResultFunction; }
            set
            {
                if (!string.Equals(_outputResultFunction, value))
                {
                    _outputResultFunction = value;
                    OnPropertyChanged("OutputResultFunction");
                }
            }
        }
        public string OutputPoint
        {
            get { return _outputPoint; }
            set
            {
                if (!string.Equals(_outputPoint, value))
                {
                    _outputPoint = value;
                    OnPropertyChanged("OutputPoint");
                }
            }
        }

        public MainWindowModel()
        {
            PlotModel = new PlotModel();
            Function = "x^2+x*y+y^2-6*x-9*y";
            _points = new List<DataPoint>();
        }
        public List<DataPoint> Points { get { return _points; } private set { _points = value; } }
        #region Command

        private DelegateCommand _beginCalculate;
        public DelegateCommand BeginCalculate
        {
            get { return _beginCalculate ?? (_beginCalculate = new DelegateCommand(CalculateExecute, CanCalculateExecute)); }
        }

        private bool CanCalculateExecute(object obj)
        {
            return true;
        }

        private void CalculateExecute(object obj)
        {

            float alpha = 1, beta = 0.5f, gamma = 2;
            float best = 0, worst, good;

            Vector bestVector = new Vector(0, 0);
            Vector averageVector = new Vector(1, 0);
            Vector worstVector = new Vector(0, 1);

            List<Vector> points = new List<Vector>();
            List<float> res = new List<float>();

            points.Add(new Vector(0, 0));
            points.Add(new Vector(1, 0));
            points.Add(new Vector(0, 1));

            ClearPoints();

            foreach (var point in points)
            {
                AddPoint(new DataPoint(point.X, point.Y));
            }
            AddPoint(new DataPoint(points[0].X, points[0].Y));

            int count = 0;
            while (count != 11)
            {
                int iMin = 0, iMax = 0, iAverage = 0; //индексы
                foreach (var point in points)
                {
                    float? val = Calculation.CalcFunc(Function, (float)point.X, (float)point.Y);
                    if (val == null)
                    {
                        MessageBox.Show("Обнаружена ошибка в формуле!");
                        return;
                    }
                    res.Add((float)val);
                }
                best = res.Min(); //поиск лучшей точки
                worst = res.Max(); //поиск худшей точки
                iMin = res.IndexOf(best); //запись индексов точек
                iMax = res.IndexOf(worst); //запись индексов точек
                //Поиск индекса среднего числа
                if (iMin == 0 && iMax == 1 || iMax == 0 && iMin == 1) iAverage = 2;
                else if (iMin == 0 && iMax == 2 || iMax == 0 && iMin == 2) iAverage = 1;
                else iAverage = 0;
                good = res[iAverage];

                Vector b = points[iMin]; //инициализация векторов
                Vector g = points[iAverage];
                Vector w = points[iMax];

                Vector mid = (b + g) / 2; //поиск средней точки между точками best и good

                //reflection
                Vector xr = mid + (mid - w) * alpha;
                if (Calculation.CalcFunc(Function, (float)xr.X, (float)xr.Y) < good) w = xr; //good
                else
                {
                    if (Calculation.CalcFunc(Function, (float)xr.X, (float)xr.Y) < worst) w = xr; //worst
                    Vector c = (w + mid) / 2;
                    if (Calculation.CalcFunc(Function, (float)c.X, (float)c.Y) < worst) w = c;
                }
                //expansion
                if (Calculation.CalcFunc(Function, (float)xr.X, (float)xr.Y) < best)
                {
                    Vector xe = mid + (xr - mid) * gamma;
                    if (Calculation.CalcFunc(Function, (float)xe.X, (float)xe.Y) < Calculation.CalcFunc(Function, (float)xr.X, (float)xr.Y))
                    {
                        w = xe;
                    }
                    else
                    {
                        w = xr;
                    }
                }
                //contraction
                if(Calculation.CalcFunc(Function, (float)xr.X, (float)xr.Y) > good)
                {
                    Vector xc = mid + (w - mid) * beta;
                    if (Calculation.CalcFunc(Function, (float)xc.X, (float)xc.Y) < worst) w = xc;
                    {
                        w = xc;
                    }
                }
                count++;
                averageVector = g;
                worstVector = w;
                bestVector = b;
                points.Clear();
                points.Add(bestVector);
                points.Add(averageVector);
                points.Add(worstVector);
                res.Clear();
                foreach (var point in points)
                {
                    AddPoint(new DataPoint(point.X, point.Y));
                }
            }
            RefreshPlot();
            OutputResultFunction = System.Math.Round(best, 4).ToString();
            OutputPoint = System.Math.Round(bestVector.X, 4).ToString() + ";" + System.Math.Round(bestVector.Y, 4).ToString();

        }
        #endregion
        public void AddPoint(DataPoint point)
        {
            _points.Add(point);
        }
        public void ClearPoints()
        {
            _points.Clear();
        }
        public void RemovePoint(DataPoint point)
        {
            _points.Remove(point);
        }
        public void RefreshPlot()
        {
            PlotModel.Series.Clear();
            LineSeries series = new LineSeries();
            series.Points.AddRange(Points);
            PlotModel.Series.Add(series);
            PlotModel.InvalidatePlot(true);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
