using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace 馬達邏輯加上sqlite
{
    public enum MotorState { Idle, Moving, Homing, Error }
    public class Motor
    {

        public double Target { get; set; } = 0;//馬達要移動到的目標點，單位為mm
        public double Speed { get; set; } = 0;//馬達終端速度，單位為mm/s
        public double NowSpeed { get; set; } = 0;//馬達目前速度，單位為mm/s
        public double Accel { get; set; } = 0;//馬達加速度，單位為mm/s^2
        public int Time { get; set; } = 0;//timer更新時間，單位為ms
        public double CurrentPos { get; set; } = 0;//馬達目前位置
        public int PulseTimes { get; set; } = 0;//馬達預期隨著時間元件產生幾次移動
        public string Name { get; set; }//馬達物件名
        public bool MovingState { get; set; } = false;//判斷馬達是否到達目標位置



        public Motor(string name) => Name = name;//建構子

        public void Motorparameter(double target, double speed, double accel, int time, double currentPos)//引入參數
        {
            Target = target;
            Speed = speed;
            Accel = accel;
            Time = time;
            CurrentPos = currentPos;
        }

        public void Moving()
        {
            // 1. 初始化環境變數
            double dt = Time / 1000.0;
            double direction = (Target - CurrentPos) >= 0 ? 1.0 : -1.0;

            // 2. 判斷目前應使用的加速度 (加速或減速)
            double remainingDist = Math.Abs(Target - CurrentPos);
            double stoppingDist = (NowSpeed * NowSpeed) / (2 * Accel);
            double currentAccel = (remainingDist <= stoppingDist) ? -Accel : Accel;

            // 3. 計算下一週期的預期速度
            double nextSpeed = NowSpeed + (currentAccel * dt * direction);

            // 速度限制截斷 (Speed Clamping)
            if (Math.Abs(nextSpeed) > Speed) nextSpeed = Speed * direction;
            if ((direction > 0 && nextSpeed < 0) || (direction < 0 && nextSpeed > 0)) nextSpeed = 0;

            // 4. 計算此週期位移量
            double deltaS = ((NowSpeed + nextSpeed) / 2.0) * dt;

            // 5. 終端校正與位置更新
            if (Math.Abs(deltaS) >= remainingDist)
            {
                CurrentPos = Target;
                NowSpeed = 0;
                MovingState = false;
                CurrentState = MotorState.Idle;
            }
            else
            {
                CurrentPos += deltaS;
                NowSpeed = nextSpeed;
            }
        }


        public MotorState CurrentState { get; set; } = MotorState.Idle;//設定預設狀態
        public void MotorUpdate()//馬達狀態機
        {
            switch (CurrentState)
            {
                case MotorState.Idle:
                    MovingState = false;
                    break;
                case MotorState.Moving:
                    Moving();
                    break;
                case MotorState.Homing:
                    CurrentState = MotorState.Moving;
                    break;
                case MotorState.Error:

                    break;
            }
        }



    }
    public class MotorConfig//馬達參數類別
    {
        public MotorConfig() { }
        public double? Target { get; set; }
        public double? Speed { get; set; }
        public double? Accel { get; set; }
        public DateTime SaveTime { get; set; }
    }
}
