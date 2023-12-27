using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Windows.Forms.DataVisualization.Charting;

namespace WindyGridWorld
{
    public partial class Form1 : Form
    {
        Random _rand = new Random();

        int episodesCount = 0;
        int _actionsCount = 4;

        double gamma = 0.5;
        double epsilon = 0.5;

        double[,][] QMatrix;

        int[] _trainedEpisode;
        double[] _sumRewards;


        public Form1()
        {
            InitializeComponent();

            ClearPanels();
            setWindLabels();
            SetCart();
        }

        private void buttonTrain_Click(object sender, EventArgs e)
        {
            buttonTrain.Enabled =
                buttonTest.Enabled = false;
            episodesCount = Convert.ToInt32(numericUpDown1.Value);

            _trainedEpisode = new int[episodesCount];
            _sumRewards = new double[episodesCount];

            InitQ();

            backgroundWorker1.RunWorkerAsync();
        }

        private void InitQ()
        {
            QMatrix = new double[Windy.WORLD_HEIGHT, Windy.WORLD_WIDTH][];

            for (int i = 0; i < Windy.WORLD_HEIGHT; i++)
            {
                for (int j = 0; j < Windy.WORLD_WIDTH; j++)
                {
                    QMatrix[i, j] = new double[] { 0, 0, 0, 0 };
                }
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i <= episodesCount; i++)
            {
                int[,] currentState = Windy.START;

                double sumRewards = 0;

                do
                {

                    int randomAction;
                    double rand = _rand.NextDouble();
                    if (rand < epsilon)
                    {
                        randomAction = _rand.Next(0, _actionsCount);
                    }
                    else
                    {
                        randomAction = 0;
                        double qAction = QMatrix[currentState[0, 0], currentState[0, 1]][0];
                        for (int a = 1; a < _actionsCount; a++)
                        {
                            double qNext = QMatrix[currentState[0, 0], currentState[0, 1]][a];
                            if (qNext > qAction)
                            {
                                randomAction = a;
                                qAction = qNext;
                            }
                        }
                    }


                    var reward = Windy.step(currentState, randomAction);

                    sumRewards += reward.Reward;
                    var train = new TrainProgress()
                    {
                        episode = i,
                        State = currentState,
                        SumRewards = sumRewards,
                    };

                    backgroundWorker1.ReportProgress(Convert.ToInt32(Math.Floor(i * 100f / episodesCount)), train);


                    double qAct0 = QMatrix[reward.State[0, 0], reward.State[0, 1]][0];
                    double qAct1 = QMatrix[reward.State[0, 0], reward.State[0, 1]][1];
                    double qAct2 = QMatrix[reward.State[0, 0], reward.State[0, 1]][2];
                    double qAct3 = QMatrix[reward.State[0, 0], reward.State[0, 1]][3];

                    double qStateAction = reward.Reward + gamma * Math.Max(Math.Max(Math.Max(qAct0, qAct1), qAct2), qAct3);

                    double lastQStateAction = QMatrix[currentState[0, 0], currentState[0, 1]][randomAction];

                    QMatrix[currentState[0, 0], currentState[0, 1]][randomAction] = Math.Round(lastQStateAction + qStateAction, 2);

                    currentState = reward.State;
                    epsilon *= 0.98;



                    Thread.Sleep(20);

                } while (!(currentState[0, 0] == Windy.GOAL[0, 0] && currentState[0, 1] == Windy.GOAL[0, 1]));

                if (i < episodesCount)
                {
                    _trainedEpisode[i] = i;
                    _sumRewards[i] = sumRewards;
                }
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ClearPanels();
            var train = (TrainProgress)e.UserState;
            int[,] currentState = train.State;
            int i = currentState[0, 0];
            int j = currentState[0, 1];

            var panel = this.Controls.Find("panel" + i.ToString() + j.ToString(), false);
            if (panel != null)
            {
                if (panel.Length > 0)
                {
                    ((Panel)panel[0]).BackColor = Color.LightGray;
                }
            }
            progressBar1.Value = e.ProgressPercentage;
            labelEpisode.Text = e.ProgressPercentage.ToString();

            DrawChart(_trainedEpisode.Take(train.episode).ToArray(), _sumRewards.Take(train.episode).ToArray());

        }

        private void setWindLabels()
        {
            for (int i = 0; i < Windy.WORLD_WIDTH; i++)
            {
                var label = this.Controls.Find("labelWind" + i.ToString(), false);
                if (label != null)
                {
                    if (label.Length > 0)
                    {
                        ((Label)label[0]).Text = Windy.WIND[i].ToString();
                    }
                }
            }
        }

        private void ClearPanels()
        {
            for (int i = 0; i < Windy.WORLD_HEIGHT; i++)
            {
                for (int j = 0; j < Windy.WORLD_WIDTH; j++)
                {

                    var panel = this.Controls.Find("panel" + i.ToString() + j.ToString(), false);
                    if (panel != null)
                    {
                        if (panel.Length > 0)
                        {
                            if (i == Windy.START[0, 0] && j == Windy.START[0, 1])
                            {
                                ((Panel)panel[0]).BackColor = Color.LightBlue;
                            }
                            else if (i == Windy.GOAL[0, 0] && j == Windy.GOAL[0, 1])
                            {
                                ((Panel)panel[0]).BackColor = Color.LightGreen;
                            }
                            else
                            {
                                ((Panel)panel[0]).BackColor = Color.White;
                            }
                        }
                    }
                }
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            buttonTrain.Enabled =
                buttonTest.Enabled = true;

            ClearPanels();

            MessageBox.Show("Training completed.");

            //DrawChart(_trainedEpisode, _sumRewards);
        }

        private void buttonTest_Click(object sender, EventArgs e)
        {
            if (QMatrix == null)
            {
                MessageBox.Show("Train the agent first before testing!");
                return;
            }

            ClearPanels();
            textBoxTest.Text = "";

            textBoxTest.Text = "[" + Windy.START[0, 0].ToString() + "," + Windy.START[0, 1].ToString() + "]";

            backgroundWorker2.RunWorkerAsync();
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            int[,] currentState = Windy.START;

            while (!(currentState[0, 0] == Windy.GOAL[0, 0] && currentState[0, 1] == Windy.GOAL[0, 1]))
            {
                int action = 0;
                double qAction = QMatrix[currentState[0, 0], currentState[0, 1]][0];
                for (int a = 1; a < _actionsCount; a++)
                {
                    double qNext = QMatrix[currentState[0, 0], currentState[0, 1]][a];
                    if (qNext > qAction)
                    {
                        action = a;
                        qAction = qNext;
                    }
                }

                var reward = Windy.step(currentState, action);

                currentState = reward.State;


                var test = new TestProgress()
                {
                    Action = action,
                    State = reward.State,
                };

                backgroundWorker2.ReportProgress(0, test);

                Thread.Sleep(500);
            }
        }

        private void backgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var test = (TestProgress)e.UserState;

            string action = "";

            switch (test.Action)
            {
                case Windy.ACTION_UP:
                    action = "UP";
                    break;
                case Windy.ACTION_DOWN:
                    action = "DOWN";
                    break;
                case Windy.ACTION_LEFT:
                    action = "LEFT";
                    break;
                case Windy.ACTION_RIGHT:
                    action = "RIGHT";
                    break;
                default:
                    break;
            }

            textBoxTest.Text += " " + action + " [" + test.State[0, 0].ToString() + "," + test.State[0, 1].ToString() + "]";

            int i = test.State[0, 0];
            int j = test.State[0, 1];

            var panel = this.Controls.Find("panel" + i.ToString() + j.ToString(), false);
            if (panel != null)
            {
                if (panel.Length > 0)
                {
                    if (i == Windy.START[0, 0] && j == Windy.START[0, 1])
                    {
                    }
                    else if (i == Windy.GOAL[0, 0] && j == Windy.GOAL[0, 1])
                    {
                    }
                    else
                    {
                        ((Panel)panel[0]).BackColor = Color.Gray;
                    }
                }
            }
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("The agent reached the goal.");
        }

        private void SetCart()
        {
            chart1.Series[0].LegendText = "Rewards during Training";
            chart1.Series[0].ChartType = SeriesChartType.Line;
            chart1.Series[0].IsValueShownAsLabel = false;
            chart1.Series[0].BorderWidth = 2;
        }
        private void DrawChart(int[] xValues, double[] yValues)
        {
            try
            {
                chart1.Series[0].Points.DataBindXY(xValues, yValues);
            }
            catch (Exception)
            {

            }
        }
    }
}
