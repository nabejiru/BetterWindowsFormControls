using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Windows.Forms;

namespace BetterFormControls.BetterControls
{
    /// <summary>
    /// 数値型テキストボックス
    /// </summary>
    public class NumberTextBox : TextBox
    {
        /// <summary>
        /// 数値精度の種類
        /// </summary>
        public enum RoundType{
            /// <summary>切り上げ</summary> 
            RoundUp,
            /// <summary>切捨て</summary> 
            RoundDown,
            /// <summary>四捨五入</summary> 
            Adjust,
        }

        private string _formatString;
        private double? _dblValue = null;
        private double _maxValue;
        private double _minValue;
        private int _digit;
        private RoundType _roundmode;

        public NumberTextBox() : base() {
            base.Text = "";
            this.ImeMode = ImeMode.Disable;
            this.TextAlign = HorizontalAlignment.Right;
            this.FormatText = "#,##0.##";
            this.MaxValue = 999999999999;
            this.MinValue = -999999999999;
            this.Digit = 2;
            this._roundmode = NumberTextBox.RoundType.Adjust;
        }



        /// <summary>
        /// MaxLengthはを取得します。
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public new int MaxLength
        {
            get
            {
                return base.MaxLength;
            }
            set { }
        }

        /// <summary>
        /// 入力マスクを取得・設定します。
        /// </summary>
        public String FormatText
        {
            get { return this._formatString; }
            set { this._formatString = value; }
        }


        /// <summary>
        /// 最大値を取得、設定します。
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public double MaxValue
        {
            get { return this._maxValue; }
            set
            {
                double val1 = Math.Abs(value);
                double val2 = Math.Abs(this.MinValue);
                if (val1.ToString().Length > val2.ToString().Length)
                {
                    base.MaxLength = val1.ToString().Length;
                }
                else
                {
                    base.MaxLength = val2.ToString().Length;
                }
                this._maxValue = value;
            }
        }

        /// <summary>
        /// 最小値を取得、設定します。
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public double MinValue
        {
            get { return this._minValue; }
            set
            {
                double val1 = Math.Abs(value);
                double val2 = Math.Abs(this.MaxValue);
                if (val1.ToString().Length > val2.ToString().Length)
                {
                    base.MaxLength = val1.ToString().Length;
                }
                else
                {
                    base.MaxLength = val2.ToString().Length;
                }
                this._minValue = value;
            }
        }


        /// <summary>
        /// 小数点以下の桁数を取得、設定します。
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public int Digit
        {
            get { return this._digit; }

            set { this._digit = value; }
        }


        public RoundType RoundMode
        {
            get { return this._roundmode; }

            set { this._roundmode = value; }
        }


        /// <summary>
        /// 指定した数値が現在設定されている最大値と最小値の範囲内か検証する
        /// </summary>
        /// <param name="decCheck">検証する数値</param>
        /// <returns>範囲内:True 範囲外:False</returns>
        /// <remarks></remarks>
        private bool bitweenMaxMin(double decCheck) {
            return ( this.MinValue <= decCheck && decCheck <= this.MaxValue );
        }


        /// <summary>
        /// 指定した精度の数値に編集 
        /// </summary>
        /// <param name="decValue">対象数値</param>
        /// <returns>処理結果</returns>
        /// <remarks>指定した精度の数値に精度を合わせる</remarks>
        private double ToRoundUp(double decValue){
            double decCoef; // 小数点位置
            decCoef = Math.Pow(10, this.Digit);

            switch(this.RoundMode) {
                case RoundType.RoundUp:      //切上げ
                    if( decValue > 0){
                        return Math.Ceiling(decValue * decCoef) / decCoef;
                    }else{
                        return Math.Floor(decValue * decCoef) / decCoef;
                    }
                case RoundType.RoundDown:     //切捨て
                    if( decValue > 0){
                        return Math.Floor(decValue * decCoef) / decCoef;
                    }else{
                        return Math.Ceiling(decValue * decCoef) / decCoef;
                    }
                case RoundType.Adjust:       //四捨五入
                    if( decValue > 0){
                        return Math.Floor((decValue * decCoef) + 0.5) / decCoef;
                    }else{
                        return Math.Ceiling((decValue * decCoef) - 0.5) / decCoef;
                    }
                default:
                    throw new NotSupportedException();
            }

        }


        /// <summary>
        /// 指定した文字列を、現在設定されている数値書式で変換します。
        /// </summary>
        /// <param name="decString">数値型に変換する文字列</param>
        /// <param name="returnedDecimal">変換した数値。変換できなかった場合はDecimal構造体の初期値が設定されます。</param>
        /// <returns>変換できた場合はTrue。変換できなかった場合はFalse。</returns>
        private bool tryPerse(string decString,ref double returnedDecimal) {
            double d;

            if( double.TryParse(decString, out d) ) {
                //数値が指定した範囲内かチェック
                if( this.bitweenMaxMin(d) ){
                    returnedDecimal = ToRoundUp(d);
                    return true;
                }else{
                    returnedDecimal = d;
                    return false;
                }
            }else{
                returnedDecimal = d;
                return false;
            }
        }

        /// <summary>
        /// 数値が有効かどうかを取得します。
        /// </summary>
        /// <returns>入力されている数値が有効ならTrue。無効ならFalse。</returns>
        [Browsable(false)] 
        public bool HasValue{
            get {
                double val = 0;
                return this.tryPerse(this.Text, ref val);
            }
        }


        /// <summary>
        /// エディット コントロールが保持している数値を取得、設定します。
        /// </summary>
        /// <value>新しく設定する数値</value>
        /// <returns>現在入力されている数値。無効であればDecimal型の初期値を返します。</returns>
        [Browsable(false), ReadOnly(true)]
        public double Value{
            get{
                return this._dblValue.GetValueOrDefault(default(double));
            }
            set{
                this._dblValue = value;

                base.Text = value.ToString(this._formatString);
            }
        }

        private double? _prevValue = null;
        public event EventHandler ChangeValueAtLostFocus;

        protected override void OnLostFocus(EventArgs e){
            double d = 0;

            if( this.tryPerse(this.Text, ref d) ){
                this._dblValue = d;
                base.Text = d.ToString(this._formatString);
            }else{
                this._dblValue = null;
                base.Text = "";
            }

            base.OnLostFocus(e);

            if(Nullable.Equals<double>(this._prevValue, this._dblValue) == false) {
                OnChangeValueAtLostFocus(new System.EventArgs());
            }
        }

        protected virtual void OnChangeValueAtLostFocus(System.EventArgs e){
            if(ChangeValueAtLostFocus!=null) ChangeValueAtLostFocus(this, e);
        }

        protected override void OnGotFocus(EventArgs e){
            this._prevValue = this._dblValue;
            //base.Text = this._dblValue.ToString
            string strWork;
            double d;


            if( this._dblValue.HasValue ) {
                d = this._dblValue.Value;
                strWork = d.ToString(this._formatString);
                base.Text = strWork;
            } else {
                //数値に変換できなければそのままにする
                base.Text = this._dblValue.ToString();
            }

            base.OnGotFocus(e);
        }


        /// <summary>
        /// 表示されているテキストを取得、または設定します。
        /// </summary>
        public override string Text{
            get{
                return base.Text;
            }

            set {
                double d = 0;
                if( this.tryPerse(value, ref d) ){
                    this._dblValue = d;
                    //書式付の数値にする
                    base.Text = d.ToString(this._formatString);
                } else {
                    //数値に変換できなければnullにする
                    this._dblValue = null;
                    // テキストはとりあえず設定するが、LostFocusで消す。
                    base.Text = value;
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            //Ctrl+Vを無効にする
            if ((keyData & Keys.Control) == Keys.Control && (keyData & Keys.KeyCode) == Keys.V) {
                this.Paste();
                return true;
            } else if ((keyData & Keys.Control) == Keys.Control && (keyData & Keys.KeyCode) == Keys.C) {

                this.Copy();
                return true;
            } else if ((keyData & Keys.Control) == Keys.Control && (keyData & Keys.KeyCode) == Keys.X) {

                this.Cut();
                return true;
            } else if ((keyData & Keys.Control) == Keys.Control && (keyData & Keys.KeyCode) == Keys.Z) {
                this.Undo();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }


        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar != '.' && e.KeyChar != '-' && e.KeyChar != (char)Keys.Back)
            {
                e.Handled = true;
            }
            base.OnKeyPress(e);
        }


        private const int WM_PASTE = 0x302;

        /// <summary>
        /// 貼付制限（数字だけ貼付）
        /// </summary>
        /// <param name="m"></param>
        /// <remarks></remarks>
        [System.Diagnostics.DebuggerStepThrough()]
        protected override void WndProc(ref Message m)
        {
            if( m.Msg == WM_PASTE) {
                IDataObject iData = Clipboard.GetDataObject();
                //文字列がクリップボードにあるか
                if( iData !=null && iData.GetDataPresent(DataFormats.Text) ) {
                    string clipStr = (string)iData.GetData(DataFormats.Text);
                    //クリップボードの文字列が数字か調べる
                    if (! Regex.IsMatch(clipStr, @"^[+-]?[0-9]+\.?[0-9]*$")) {
                        return;
                    }
                }
            }
            base.WndProc(ref m);
        }
    }

}
