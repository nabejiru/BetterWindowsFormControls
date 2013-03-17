using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace BetterFormControls.BetterControls
{
    public class LimitableTextBox : TextBox
    {
        private const int WM_PASTE = 0x302;
        private const int WM_CHAR = 0x102;

        private Encoding _sjis = Encoding.GetEncoding("Shift_JIS");
        private LimitableTextBox.CharacterType _charType = CharacterType.AllChars;
        private int _maxByteLength = 0;
        private bool _allowNewLine = true;

        /// <summary>文字を表します。</summary> 
        public enum CharacterType
        {
            /// <summary>すべての文字列</summary> 
            AllChars,
            /// <summary>数字</summary> 
            Number,
            /// <summary>半角英字</summary> 
            Alphabet,
            /// <summary>半角の英字、または数字）</summary> 
            AlphabetAndNumber,
            /// <summary>すべてのASCII文字</summary> 
            Ascii,
        }


        public LimitableTextBox()
            : base()
        {
            // MaxByteLengthプロパティを使用するため、従来のMaxLengthプロパティは0に固定します。
            base.MaxLength = 0;
        }

        //CHECK : フォーカス系のイベントはどこかで共通化する？
        private string _textAtGotFocus;

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);

            this._textAtGotFocus = this.Text;
        }

        protected override void OnLostFocus(System.EventArgs e)
        {
            base.OnLostFocus(e);

            if (string.Equals(this._textAtGotFocus, this.Text) == false)
            {
                this.OnLostFocusWithValueChanged(new EventArgs());
            }
        }

        /// <summary>
        /// フォーカスが無くなった際に、フォーカス取得前のテキストが変更されていた場合に発生します。
        /// </summary>
        public event EventHandler LostFocusWithValueChanged;

        /// <summary>
        /// LostFocusWithValueChanged イベントを発生させます。
        /// </summary>
        protected virtual void OnLostFocusWithValueChanged(EventArgs e)
        {
            if (LostFocusWithValueChanged != null) LostFocusWithValueChanged(this, e);
        }

        [Category("動作"),
         DefaultValue(0),
         Description("エディット コントロールに入力できる文字列の最大バイト数を取得します。この値は設定しても変化はありません。")]
        public override int MaxLength
        {
            get
            {
                return this.MaxByteLength;
            }
            set
            {
                //何もしません
            }
        }


        /// <summary>
        /// エディット コントロールに入力できる文字列の最大バイト数を取得、設定します。
        /// </summary>
        /// <value>エディット コントロールに入力できる文字列の最大バイト数</value>
        /// <returns>エディット コントロールに入力できる文字列の最大バイト数</returns>
        [Category("動作"),
         DefaultValue(0),
         Description("エディット コントロールに入力できる文字列の最大バイト数を指定します。")]
        public int MaxByteLength
        {
            get
            {
                return this._maxByteLength;
            }
            set
            {
                this._maxByteLength = value;
            }
        }


        /// <summary>
        /// 入力文字列のバイト数による入力制限をするかどうかを取得します。
        /// </summary>
        /// <returns>入力制限を行う場合true。バイト数による制限が無い場合はfalse。</returns>
        protected virtual bool CheckingByteLength
        {
            get
            {
                return this.MaxByteLength > 0;
            }
        }

        /// <summary>
        /// エディットコントロールに入力できる文字の種類を取得、設定します。
        /// </summary>
        /// <value>エディットコントロールに入力できる文字の種類</value>
        /// <returns>エディットコントロールに入力できる文字の種類</returns>
        [Category("動作"),
         DefaultValue(typeof(CharacterType), "CharacterType.AllChars"),
         Description("エディット コントロールに入力できる文字の種類を指定します。")]
        public CharacterType PermittedCharsType
        {
            get
            {
                return this._charType;
            }

            set
            {
                this._charType = value;

                //IMEモードを変更
                if (value == LimitableTextBox.CharacterType.AllChars)
                {
                    this.ImeMode = ImeMode.NoControl;
                }
                else
                {
                    this.ImeMode = ImeMode.Disable;
                }
            }
        }


        /// <summary>
        /// PermittedCharsTypeに応じた正規表現のパターンを取得します。
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        private string limittedRegexPattern
        {
            get
            {
                string pattern = "";

                switch (this._charType)
                {
                    case CharacterType.Number:
                        pattern = "[0-9]";
                        break;
                    case CharacterType.Ascii:
                        pattern = @"[\!-~]";
                        break;
                    case CharacterType.AlphabetAndNumber:
                        pattern = "[a-zA-Z0-9]";
                        break;
                    case CharacterType.Alphabet:
                        pattern = "[a-zA-Z]";
                        break;
                }

                return pattern;
            }
        }

        /// <summary>
        /// 指定した文字が制限されていないかどうかを検証します。
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private bool isPermitedString(string target)
        {
            if (this.PermittedCharsType == CharacterType.AllChars)
            {
                return true;
            }
            else
            {
                // 指定された文字種で入力されているかチェックする
                return Regex.IsMatch(target, this.limittedRegexPattern);
            }
        }


        [Category("動作"),
         DefaultValue(true),
         Description("改行の入力を許すかどうかを指定します。このプロパティによる動作はMultiLineプロパティがtrueの場合にしか影響しません。")]
        public bool AllowNewLine
        {
            get { return this._allowNewLine; }
            set { this._allowNewLine = value; }
        }

        [DebuggerStepThrough()]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_CHAR:
                    KeyPressEventArgs eKeyPress = new KeyPressEventArgs((char)m.WParam.ToInt32());
                    OnChar(eKeyPress);

                    if (eKeyPress.Handled) return;
                    break;
                case WM_PASTE:
                    if (this.ReadOnly == false)
                    {
                        this.OnPaste(new EventArgs());
                    }

                    return;
            }

            base.WndProc(ref m);
        }


        /// <summary>
        /// OnCharイベント（キーボード押下イベント）を発生させます。
        /// </summary>
        /// <param name="e">イベント情報を格納しているKeyPressEventArgsオブジェクト</param>
        protected virtual void OnChar(KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;

            //bool isHalfChar = true;
            byte[] charBytes = this._sjis.GetBytes(e.KeyChar.ToString().ToCharArray());

            //指定された文字種で入力されているかチェック
            if (this.isPermitedString(e.KeyChar.ToString()) == false)
            {
                e.Handled = true;
                return;
            }

            if (this.Multiline && this.AllowNewLine == false)
            {
                if (e.KeyChar == '\r' || e.KeyChar == '\n')
                {
                    e.Handled = true;
                    return;
                }
            }

            // バイト数の入力制限
            if (this.CheckingByteLength)
            {
                int textByteCount = this._sjis.GetByteCount(this.Text);
                int inputByteCount = this._sjis.GetByteCount(e.KeyChar.ToString());
                int selectedTextByteCount = this._sjis.GetByteCount(this.SelectedText);

                if (textByteCount + inputByteCount - selectedTextByteCount > this.MaxByteLength)
                {
                    e.Handled = true;
                }
            }

        }


        /// <summary>
        /// OnPasteイベント（クリップボードの貼り付け）を発生させます。
        /// </summary>
        /// <param name="e">イベント情報を格納しているSystem.EventArgsオブジェクト</param>
        protected virtual void OnPaste(EventArgs e)
        {
            object clipboardText = Clipboard.GetDataObject().GetData(System.Windows.Forms.DataFormats.Text);
            if (clipboardText == null) return;

            string pastedString = clipboardText.ToString();

            if (this.isPermitedString(pastedString) == false) return;

            if (this.Multiline && this.AllowNewLine == false)
            {
                pastedString = pastedString.Replace("\n", "");
                pastedString = pastedString.Replace("\r", "");
            }


            // バイト数の入力制限
            if (this.CheckingByteLength)
            {
                int textByteCount = this._sjis.GetByteCount(this.Text);
                int inputByteCount = this._sjis.GetByteCount(pastedString);
                int selectedTextByteCount = this._sjis.GetByteCount(this.SelectedText);
                int remainByteCount = this.MaxByteLength - (textByteCount - selectedTextByteCount);

                if (remainByteCount <= 0) return;

                if (remainByteCount >= inputByteCount)
                {
                    this.SelectedText = pastedString;
                }
                else
                {
                    this.SelectedText = LeftB(pastedString, remainByteCount);
                }
            }
            else
            {
                this.SelectedText = pastedString;
            }

        }


        public static string LeftB(string sData, int nLen)
        {
            Encoding oSJisEncoding = Encoding.GetEncoding("Shift_JIS");
            byte[] nByteAry = oSJisEncoding.GetBytes(sData);

            if (nByteAry.Length <= nLen)
            {
                return sData;
            }
            else
            {
                string sLeftStr = oSJisEncoding.GetString(nByteAry, 0, nLen);
                int nLastPos = sLeftStr.Length - 1;
                if (0 < nLastPos && sData[nLastPos] != sLeftStr[nLastPos])
                {
                    sLeftStr = sLeftStr.Substring(0, nLastPos);
                }
                return sLeftStr;
            }
        }

    }
}
