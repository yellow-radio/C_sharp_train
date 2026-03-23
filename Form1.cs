using System.Text.Json;
using System.Xml.Linq;

namespace day20_綜合實作_
{

    
    public partial class Form1 : Form
    {
        

        public MotorConfig _recipe { get; private set; }//預宣告參數物件變數，方便不同函數傳遞物件
        public Motor _motor { get; private set; }//預宣告馬達狀態機物件變數，方便不同函數傳遞物件
        public double _currentPos { get; private set; } = 0.0 ;//馬達位置
        public bool _isMoving { get; private set; } = false;//馬達是否在運動
        public double TargetMax { get; } = 20;//最大移動行程
        private void AddLog(string message)//帶時間戳的log寫入函數
        {
            // 取得當前時間，格式為 10:30:05
            string timeStamp = DateTime.Now.ToString("HH:mm:ss");

            // 組合訊息並新增
            lstLogsBox.Items.Add($"[{timeStamp}] {message}");

            // 自動捲動到最下方，確保使用者看到最新狀態
            lstLogsBox.TopIndex = lstLogsBox.Items.Count - 1;
        }
        private void UpdateRecipeFromUI()//能夠實時更新參數並寫入變數的方法
        {
            MotorConfig recipe = new MotorConfig()
            {
                Target = (double)TargetUpDown.Value,
                Speed = (double)SpeedUpDown.Value,
                Accel = (double)AccelUpDown.Value,
                SaveTime = DateTime.Now
            };
            _recipe = recipe;
        }
        public Form1()
        {
            InitializeComponent();
            AddLog("程式已開啟");
            _motor = new Motor("馬達1");
        }

        private void lblCurrentPoslabel_Click(object sender, EventArgs e)//馬達位置顯示欄位
        {

        }

        private void TargetUpDown_ValueChanged(object sender, EventArgs e)//目標位置欄位
        {

        }

        private void SpeedUpDown_ValueChanged(object sender, EventArgs e)//初速度欄位
        {

        }

        private void AccelUpDown_ValueChanged(object sender, EventArgs e)//加速度欄位
        {

        }

        private void btnStart_Click(object sender, EventArgs e)//啟動按鈕
        {
            
            AddLog("啟動按鈕被按下，寫入參數");
            UpdateRecipeFromUI();
            //_currentPos = _recipe.Target ?? 0.0;//如果_recipe.Target為null，則將_currentPos設為0
            _motor.Motorparameter(_recipe.Target ?? 0, _recipe.Speed ?? 0, _recipe.Accel ?? 0, timer1.Interval, _currentPos);
            if (_recipe.Speed <= 0 || _recipe.Accel <= 0)
            {
                AddLog("速度或加速度不可小於等於0，請重新輸入");
                return;
            }
            else if (Math.Abs(_currentPos - (double)_recipe.Target) > TargetMax)
            {
                AddLog("已超過單次移動行程，請重新輸入距離");
                _motor.CurrentState = MotorState.Error;
                return;
            }
            else if (Math.Abs(_currentPos - (double)_recipe.Target) <= TargetMax)
            {
                AddLog("準備開始運動");
                _isMoving = true;
                timer1.Enabled = true;
                _motor.MovingState = true;
                _motor.CurrentState = MotorState.Moving;
            }
            else
            {
                AddLog("未知運行情況，終止運行");
                _motor.CurrentState = MotorState.Error;
                return;
            }
            lockstate();
        }

        private void btnStop_Click(object sender, EventArgs e)//停止按鈕
        {
            AddLog("停止按鈕被按下，馬達強制減速");
            _motor.CurrentState = MotorState.Idle;
            timer1.Enabled = false;
            _motor.MotorUpdate();
            _currentPos = _motor.CurrentPos;
            lblCurrentPoslabel.Text = $"馬達當前位置：{_motor.CurrentPos:F2}mm";
            lockstate();
        }

        private void btnHome_Click(object sender, EventArgs e)//回歸原位按鈕
        {
            
            AddLog("回歸原點按鈕被按下，馬達正在回歸原位");
            AddLog("準備開始運動");
            _recipe.Target = 0;
            _recipe.Speed = 1;
            _recipe.Accel = 1;
            _motor.Motorparameter(_recipe.Target ?? 0, _recipe.Speed ?? 0, _recipe.Accel ?? 0, timer1.Interval, _currentPos);
            _isMoving = true;
            timer1.Enabled = true;
            _motor.MovingState = true;
            _motor.CurrentState = MotorState.Homing;
            _motor.MotorUpdate();
            _currentPos = _motor.CurrentPos;
            lblCurrentPoslabel.Text = $"馬達當前位置：{_motor.CurrentPos:F2}mm";
            lockstate();
            //AddLog("馬達已回到原位");
            //_motor.CurrentState = MotorState.Idle;
        }

        private void btnReset_Click(object sender, EventArgs e)//緊急停止按紐
        {
            AddLog("重置中...");
            _motor.CurrentState = MotorState.Idle;
            lblCurrentPoslabel.Text = $"馬達當前位置：{_motor.CurrentPos:F2}mm";
            _motor.MovingState = false;
            timer1.Enabled = false;
        }
        private async void btnSave_Click(object sender, EventArgs e)//儲存參數按鈕
        {
            
            AddLog("儲存按鈕被按下，準備儲存Recipe");
            
                UpdateRecipeFromUI();
                if (_recipe != null && (_recipe.Target ?? 0) != 0 && (_recipe.Speed ?? 0) != 0 && (_recipe.Accel ?? 0) != 0)
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Filter = "JSON 檔案 (*.json)|*.json";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                    // 序列化為字串
                    await Task.Run(() =>
                    {
                        string jsonString = JsonSerializer.Serialize(_recipe, new JsonSerializerOptions { WriteIndented = true });
                        // 寫入檔案
                        File.WriteAllText(saveFileDialog.FileName, jsonString);
                    });
                    AddLog("Recipe已儲存");
                    }
                }
                else
                {
                    AddLog("輸入參數格式錯誤");
                }
            

        }

        private async void btnLoad_Click(object sender, EventArgs e)//載入參數按鈕
        {
            AddLog("準備讀取Recipe");
            
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "JSON 檔案 (*.json)|*.json";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string jsonString = await File.ReadAllTextAsync(openFileDialog.FileName);
                        // 反序列化回物件
                        MotorConfig? loadedRecipe = JsonSerializer.Deserialize<MotorConfig>(jsonString);

                        if (loadedRecipe != null)
                        {
                            TargetUpDown.Value = (decimal)(loadedRecipe.Target ?? 0);
                            SpeedUpDown.Value = (decimal)(loadedRecipe.Speed ?? 0);
                            AccelUpDown.Value = (decimal)(loadedRecipe.Accel ?? 0);
                            _recipe = loadedRecipe;
                            AddLog("Recipe 讀取並套用成功");
                        }
                    }
                    catch (Exception ex)
                    {
                        AddLog("讀取失敗");
                    }
                }
                else
                {
                    AddLog("未選擇文件，或其他例外情況");
                }
            
        }

        private void lockstate()
        {
            if (_motor.CurrentState == MotorState.Moving || _motor.CurrentState == MotorState.Homing)//當馬達狀態為運動或回歸原位時，禁用按鈕和參數輸入框
            {
                btnStart.Enabled = false;
                btnLoad.Enabled = false;
                TargetUpDown.Enabled = false;
                SpeedUpDown.Enabled = false;
                AccelUpDown.Enabled = false;
            }
            else//當馬達不在運動或回歸原位時，啟用按鈕和參數輸入框
            {
                btnStart.Enabled = true;
                btnLoad.Enabled = true;
                TargetUpDown.Enabled = true;
                SpeedUpDown.Enabled = true;
                AccelUpDown.Enabled = true;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            
            if (_motor.MovingState)
            {
                // 1. 驅動邏輯演進
                _motor.MotorUpdate();

                // 2. UI 僅負責讀取並呈現結果
                lblCurrentPoslabel.Text = $"馬達當前位置：{_motor.CurrentPos:F2}mm";
                _currentPos = _motor.CurrentPos; // 同步 Form1 內部的座標備份
            }
            else
            {
                lockstate();
                timer1.Enabled = false;
                AddLog("任務執行完畢。");
            }
        }

        private void lstLogsBox_SelectedIndexChanged(object sender, EventArgs e)//log輸出框
        {

        }
        
    }
    
}
