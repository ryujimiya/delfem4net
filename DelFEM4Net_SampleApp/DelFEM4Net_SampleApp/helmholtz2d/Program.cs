using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace helmholtz2d
{
    /// <summary>
    /// アプリケーション
    /// </summary>
    class Program
    {
        /// <summary>
        /// アプリケーションのエントリーポイント
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            try
            {
                // メインロジックの生成
                using (MainLogic mainLogic = new MainLogic())
                {
                    // アプリケーションの終了イベントハンドラを設定する
                    AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
                    {
                        Console.WriteLine("Process exiting");
                        System.Diagnostics.Debug.WriteLine("Process exiting");
                        // メインロジックの破棄処理を呼び出す
                        mainLogic.Dispose();
                    };

                    // メインロジックを実行
                    mainLogic.Run();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message + " " + exception.StackTrace);
                System.Diagnostics.Debug.WriteLine(exception.Message + " " + exception.StackTrace);
            }
        }
    }
}
