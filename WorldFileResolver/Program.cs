using System;
using CommandLine;
using System.Collections.Generic;
using Microsoft.VisualBasic.FileIO;
using System.IO;

namespace WorldFileResolver
{
	class MainClass
	{
		enum ResolverErrorType {
			FROM_CODE_AND_TEXT = 1,
			TO_CODE_AND_TEXT,
			NEED_INPUT,
			NONEXIST_INPUT,
			EXIST_OUTPUT,
			SHORT_COLUMN,
			BAD_ARGUMENT
		};

		static Dictionary<ResolverErrorType,string> ErrorMessage = new Dictionary<ResolverErrorType,string>(){
			{ResolverErrorType.FROM_CODE_AND_TEXT, "from textと from codeの同時指定は許されていません。"},
			{ResolverErrorType.TO_CODE_AND_TEXT,   "to textと to codeの同時指定は許されていません。"},
			{ResolverErrorType.NEED_INPUT,         "入力ファイルの指定が必要です。"},
			{ResolverErrorType.NONEXIST_INPUT,     "入力ファイルが存在しません。"},
			{ResolverErrorType.EXIST_OUTPUT,       "出力ファイルが存在します。"},
			{ResolverErrorType.SHORT_COLUMN,       "入力ファイルの列数が不足です。"},
			{ResolverErrorType.BAD_ARGUMENT,       "コマンドライン引数の解析に失敗。"}
		};

		public static void Main (string[] args)
		{
			var opts = new Options();
			bool isSuccess = CommandLine.Parser.Default.ParseArguments(args, opts);
			if (isSuccess)
			{
				if (opts.help) {
					Console.WriteLine ("WorldFileResolver.exe [-f fromProjText|-g fromepsgCode] [-t toProjText|-u toEpsgCode] [-o outputFile] inputFile");
					Environment.Exit(0);
				} 

				var fText = opts.fromProjText;
				var fCode = opts.fromEpsgCode;
				var tText = opts.toProjText;
				var tCode = opts.toEpsgCode;

				Resolver res;

				if (fText != null || fCode != 0) {
					if (fText != null && fCode != 0)
						HandleError (ResolverErrorType.FROM_CODE_AND_TEXT);

					if (tText != null || tCode != 0) {
						if (tText != null && tCode != 0)
							HandleError (ResolverErrorType.TO_CODE_AND_TEXT);

						if (fText != null) {
							res = tText != null ? new Resolver (fText, tText) : new Resolver (fText, tCode);
						} else {
							res = tText != null ? new Resolver (fCode, tText) : new Resolver (fCode, tCode);
						}
					} else {
						res = new Resolver ();
					}
				} else if (tText != null || tCode != 0) {
					if (tText != null && tCode != 0)
						HandleError (ResolverErrorType.TO_CODE_AND_TEXT);
					res = tText != null ? new Resolver (tText) : new Resolver (tCode);
				} else {
					res = new Resolver ();
				}

				if (opts.InputFiles.Count < 1)
					HandleError (ResolverErrorType.NEED_INPUT);

				var rFile = opts.InputFiles [0];
				if (!File.Exists (rFile))
					HandleError (ResolverErrorType.NONEXIST_INPUT);

				var oFile = opts.outputFile;
				if (oFile != null && File.Exists (oFile))
					HandleError (ResolverErrorType.EXIST_OUTPUT);
					
				TextFieldParser parser = new TextFieldParser(rFile);
				parser.TextFieldType = FieldType.Delimited;
				// 区切り文字はコンマ
				parser.SetDelimiters(","); 

				while (!parser.EndOfData) {
					// 1行読み込み
					string[] row = parser.ReadFields();
					if (row.Length < 4)
						HandleError (ResolverErrorType.SHORT_COLUMN);

					res.AddMapPoint (double.Parse(row[0]),double.Parse(row[1]),double.Parse(row[2]),double.Parse(row[3]));
				}

				var result = res.Resolve ();

				if (oFile != null) {
					var sw = new StreamWriter (oFile);
					sw.WriteLine ("{0}", result.A);
					sw.WriteLine ("{0}", result.D);
					sw.WriteLine ("{0}", result.B);
					sw.WriteLine ("{0}", result.E);
					sw.WriteLine ("{0}", result.C);
					sw.WriteLine ("{0}", result.F);
					sw.Close ();
				} else {
					Console.WriteLine ("{0}", result.A);
					Console.WriteLine ("{0}", result.D);
					Console.WriteLine ("{0}", result.B);
					Console.WriteLine ("{0}", result.E);
					Console.WriteLine ("{0}", result.C);
					Console.WriteLine ("{0}", result.F);
				}

				Environment.Exit (0);
			}
			else
			{
				HandleError (ResolverErrorType.BAD_ARGUMENT);
			}
		}

		private static void HandleError (ResolverErrorType errType) {
			Console.Write ("エラー: ");
			Console.WriteLine (ErrorMessage [errType]);
			Environment.Exit (1);
		}
	}

	class Options {
		[CommandLine.ValueList(typeof(List<string>), MaximumElements=1)]
		public IList<string> InputFiles
		{
			get;
			set;
		}

		[CommandLine.Option('f',"fromtext", DefaultValue=null)]
		public string fromProjText
		{
			get;
			set;
		}

		[CommandLine.Option('g',"fromcode", DefaultValue=0)]
		public int fromEpsgCode
		{
			get;
			set;
		}

		[CommandLine.Option('t',"totext", DefaultValue=null)]
		public string toProjText
		{
			get;
			set;
		}

		[CommandLine.Option('u',"tocode", DefaultValue=0)]
		public int toEpsgCode
		{
			get;
			set;
		}

		[CommandLine.Option('o',"output", DefaultValue=null)]
		public string outputFile
		{
			get;
			set;
		}
			
		[CommandLine.Option('h',"help", DefaultValue=false)]
		public bool help
		{
			get;
			set;
		}
	}
}
