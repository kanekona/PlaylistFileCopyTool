using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PlaylistCopy
{
    public partial class Form1 : Form
    {
        // コピー中のログテキスト更新用デリゲート
        delegate bool UpdateTextDelegate();

        //! コピー実行中かどうか
        bool bIsRunCopy;
        //! ログテキスト
        string logText;

        public Form1()
        {
            InitializeComponent();

            // 初期時のボタンの表示内容変更
            button1.Text = "OpenPlaylist";
            button2.Text = "OpenCopyFolder";
            button3.Text = "Copy";
            Text = "WindowsMediaPlayer Playlist Music File Copy Tool";
            
            // コピー実行中フラグをFalse
            bIsRunCopy = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // コピー実行中は変更をさせない
            if(bIsRunCopy)
            {
                return;
            }
            // ファイル指定ダイアログを表示する
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // コピー実行中は変更させない
            if(bIsRunCopy)
            {
                return;
            }
            // フォルダ指定ダイアログを表示する
            if(folderBrowserDialog1.ShowDialog()== DialogResult.OK)
            {
                textBox2.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(bIsRunCopy)
            {
                return;
            }
            logText = "";
            UpdateText();
            bIsRunCopy = true;
            System.Threading.Thread copyThread = new System.Threading.Thread(() => { CopyToFile(); });
            copyThread.Start();
        }

        private void CopyToFile()
        {
            // 万が一にでも内容が変更されると困るので最初に保存しておく
            string playlistFilePath = textBox1.Text;
            string CopyFolderPath = textBox2.Text;

            // プレイリストの中身を１列ごとに配列に格納
            string[] playlistLines = System.IO.File.ReadAllLines(playlistFilePath);

            // コピーするパスのリスト
            List<string> copyPathList = new List<string>();

            foreach (string it in playlistLines)
            {
                // 音楽データのファイル名かどうかチェックする
                if (it.IndexOf("<media src=") >= 0)
                {
                    // 音楽データのファイルパスを取得して追加
                    int first = it.IndexOf("\"") + 1;
                    int second = it.IndexOf("\"", first);
                    copyPathList.Add(it.Substring(first, second - first));
                }
            }

            // プレイリストのある場所のパスを取得
            string playlistPath = playlistFilePath.Substring(0, playlistFilePath.LastIndexOf("\\") + 1);

            foreach (string copyPath in copyPathList)
            {
                // ファイルの名前を取得
                int fileNameTopIndex = copyPath.LastIndexOf("\\");
                string fileName = copyPath.Substring(fileNameTopIndex, copyPath.Length - fileNameTopIndex);
                // 中身変更するため一度変更可能変数へコピーする
                string path = copyPath;

                // 専用文字列変換をもとに戻す
                path = path.Replace("&apos;", "\'");
                path = path.Replace("&amp;", "&");

                // 上書き可でコピーする
                System.IO.File.Copy(playlistPath + path, CopyFolderPath + fileName, true);

                // ログ更新
                logText += "copy to " + playlistPath + path + " => " + CopyFolderPath + fileName + "\n";
                Invoke(new UpdateTextDelegate(UpdateText));
            }
            // コピー処理終了
            bIsRunCopy = false;
            logText += "=== Finish Copy ===";
            Invoke(new UpdateTextDelegate(UpdateText));
        }

        private bool UpdateText()
        {
            // ログの内容をテキストボックスへ反映させる
            richTextBox1.Text = logText;
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.Focus();
            richTextBox1.ScrollToCaret();
            return true;
        }
    }   
}
