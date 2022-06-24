using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace olhrkcl
{
    class Client
    {

        /// <summary>
        /// call back for command line option
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        internal delegate int OptionHandler(string option);

        /// <summary>
        /// run the program
        /// </summary>
        /// <returns></returns>
        internal delegate int RunHandler();


        /// <summary>
        /// callback for reading data to send to processing server
        /// </summary>
        /// <param name="dataName"></param>
        /// <returns></returns>
        internal delegate byte[] ReadDataHanler(string dataName);


        class Option
        {
            internal char ShortOption;
            internal string LongOption;
            internal string Description;
            internal OptionHandler Handler;
            internal bool NeedParameter;
            internal Gnu.Getopt.Argument OptionArgs;
            internal Gnu.Getopt.LongOpt LongOpt
            {
                get
                {
                    Gnu.Getopt.LongOpt result;
                    result = null;
                    if (LongOption != null)
                    {

                        result = new Gnu.Getopt.LongOpt(LongOption, OptionArgs, null, ShortOption);
                    }

                    return result;
                }
            }


            /// <summary>
            /// constructor
            /// </summary>
            /// <param name="shortOption"></param>
            /// <param name="longOption"></param>
            /// <param name="description"></param>
            internal Option(char shortOption, string longOption, string description,
                Gnu.Getopt.Argument optionArgs, bool needParameter, OptionHandler handler)
            {
                this.ShortOption = shortOption;
                this.LongOption = longOption;
                this.Description = description;
                this.Handler = handler;
                this.OptionArgs = optionArgs;
                this.NeedParameter = needParameter;
            }
        }

        /// <summary>
        /// command options
        /// </summary>
        private IList<Option> OptionsValue;

        /// <summary>
        /// command options
        /// </summary>
        private IList<Option> Options
        {
            get
            {
                IList<Option> result;
                result = null;
                if (OptionsValue == null)
                {
                    OptionsValue = new List<Option>();

                    OptionsValue.Add(new Option('h', "help", Properties.Resources.HelpDesc, Gnu.
                        Getopt.Argument.No, false, HandleHelp));
                    OptionsValue.Add(new Option('s', "status", Properties.Resources.ReadStatusDesc, Gnu.
                        Getopt.Argument.No, false, HandleReadStatus));
                    OptionsValue.Add(new Option('f', "send-data", Properties.Resources.SendDataDesc, Gnu.Getopt.Argument.Optional,
                        true, HandleSendData));
                    OptionsValue.Add(new Option('t', "data-type", Properties.Resources.DataTypeDesc, Gnu.Getopt.Argument.Optional,
                        true, HandleDataType));
                }
                result = OptionsValue;

                return result;
            }
        }

        /// <summary>
        /// option code, option handler map
        /// </summary>
        private IDictionary<int, OptionHandler> CodeOptionHandlerValue;

        /// <summary>
        /// option code, option handler map
        /// </summary>
        private IDictionary<int, OptionHandler> CodeOptionHandler
        {
            get
            {
                if (CodeOptionHandlerValue == null)
                {
                    CodeOptionHandlerValue = new Dictionary<int, OptionHandler>();

                    foreach (var option in Options)
                    {
                        CodeOptionHandlerValue[option.ShortOption] = option.Handler;
                    }
                }
                return CodeOptionHandlerValue;
            }
        }


        ReadDataHanler DataReader
        {
            get
            {
                string dataType;
                dataType = GetDataType(DataFile);
                ReadDataHanler result;
                if (dataType != null)
                {
                    dataType = dataType.ToLower();
                }
                if (dataType == null || "json" == dataType)
                {
                    result = ReadJson;
                }
                else
                {
                    result = ReadRaw;
                }
                return result;
            }
        }


        /// <summary>
        /// program name
        /// </summary>
        internal string ProgramName;


        /// <summary>
        /// main task callback
        /// </summary>
        internal RunHandler Run;


        private oldlclr.Client DataLink;


        /// <summary>
        /// data type for processing;
        /// </summary>
        private string DataType;


        /// <summary>
        /// data file for processing
        /// </summary>
        private string DataFile;




        Client(string programName)
        {
            DataLink = new oldlclr.Client();
            this.ProgramName = programName;
            this.Run = RunNothing;
            this.DataFile = "-";




        }

        /// <summary>
        /// run nothing
        /// </summary>
        /// <returns></returns>
        private int RunNothing()
        {
            int result;
            result = 0;

            return result;
        }

        /// <summary>
        /// print out help message
        /// </summary>
        /// <returns></returns>
        private int RunHelp()
        {
            int result;
            result = 0;
            System.Console.Out.Write(CreateHelpLine());
            System.Console.Out.Flush();

            return result;
        }

        /// <summary>
        /// help option handler
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        private int HandleHelp(string option)
        {
            int result;
            result = 0;


            Run = RunHelp;


            return result;

        }

        string CreateHelpLine()
        {
            IList<string> lines;
            lines = new List<string>();

            SimpleStrTable strTable;

            strTable = new SimpleStrTable();

            foreach (var opt in Options)
            {
                string[] desclines;
                desclines = opt.Description.Split(new string[] { "\n", "\r", "\r\n" }, StringSplitOptions.None);
                IList<string> options;
                options = new List<string>();
                options.Add(string.Format("-{0}", opt.ShortOption));
                if (opt.LongOption != null)
                {
                    options.Add(string.Format("--{0}", opt.LongOption));
                }
                strTable.Add(options.ToArray(), desclines);

            }

            string usageLine;
            usageLine = string.Format(Properties.Resources.UsageFormat, ProgramName);

            return string.Format("{0}\n{1}", usageLine, strTable);

        }


        /// <summary>
        /// Handle read status option
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        private int HandleReadStatus(string option)
        {
            int result;
            result = 0;

            this.Run = RunReadStatus;

            return result;
        }

        /// <summary>
        /// Run read status
        /// </summary>
        /// <returns></returns>
        private int RunReadStatus()
        {
            int result;

            result = 0;

            oldlclr.Status status;
            status = null;

            if (this.DataLink.Connect())
            {
                status = this.DataLink.GetStatus();
                this.DataLink.Disconnect();
            }


            string message;
            message = null;
            if (status != null)
            {
                message = status.ToJson();
            }
            else
            {
                message = oldlclr.Error.GetErrorAsJson();
            }

            if (message != null)
            {
                System.Console.Out.Write(message);
                System.Console.Out.Flush();
            }
            if (status != null)
            {
                status.Dispose();
            }

            return result;
        }


        /// <summary>
        /// handle send data option
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        private int HandleSendData(string option)
        {
            int result;
            result = 0;
            this.DataFile = option;
            this.Run = RunSendData;
            return result;
        }

        /// <summary>
        /// run send data
        /// </summary>
        /// <returns></returns>
        private int RunSendData()
        {
            int result;
            result = 0;

            string dataName;
            dataName = this.DataFile;

            byte[] data;
            data = DataReader(dataName);


            if (data != null)
            {
                bool state;

                if (DataLink.Connect())
                {
                    state = DataLink.LoadData(data, dataName);
                    if (state)
                    {
                        oldlclr.Error.SetError(oldlclr.ErrorCode.NO_ERROR);
                    }
                    DataLink.Disconnect();
                }

                result = oldlclr.Error.GetError();
                System.Console.Out.Write(oldlclr.Error.GetErrorAsJson());
                System.Console.Out.Flush();
            }

            return result;
        }

        /// <summary>
        /// handle data type option
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        private int HandleDataType(string option)
        {
            int result;
            result = 0;

            this.DataType = option;

            return result;

        }


        /// <summary>
        /// handle data name
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        private int HandleDataName(string option)
        {
            int result;
            result = 0;
            this.DataFile = option;
            return result;
        }


        /// <summary>
        /// check dataname whether stdin file or not.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private bool IsStdIn(string dataName)
        {
            bool result;

            result = false;

            result = dataName == null;
            if (!result)
            {
                result = "-" == dataName;
            }

            return result;
        }

        /// <summary>
        /// read data from dataFile
        /// </summary>
        /// <param name="dataName"></param>
        /// <returns></returns>
        private byte[] ReadData(string dataName)
        {
            
            System.IO.Stream st;
            st = null;

            bool closeHandle;
            if (IsStdIn(dataName))
            {
                closeHandle = false;

                st = System.Console.OpenStandardInput();
            }
            else
            {
                try
                {
                    st = new System.IO.FileStream(dataName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                }
                catch (Exception ex)
                {

                }


                closeHandle = true;
            }

            byte[] result;
            result = null;

            if (st != null)
            {
                System.IO.MemoryStream memStr;
                memStr = new System.IO.MemoryStream();
                try
                {
                    byte[] buffer;
                    buffer = new byte[1024];
                    while (true)
                    {
                        int readSize;
                        readSize = st.Read(buffer, 0, buffer.Length);
                        if (readSize > 0)
                        {
                            memStr.Write(buffer, 0, readSize);
                        }
                        else
                        {
                            break;
                        }
                    }
                    result = new byte[memStr.Length];
                    memStr.Seek(0, System.IO.SeekOrigin.Begin);
                    memStr.Read(result, 0, result.Length);
                }
                catch (Exception ex)
                {
                }

            }

            if (closeHandle)
            {
                if (st != null)
                {
                    st.Close();
                }
            }

            return result;

        }
        

        /// <summary>
        /// read json data from dataName
        /// </summary>
        /// <param name="dataName"></param>
        /// <returns></returns>
        private byte[] ReadJson(string dataName)
        {
            byte[] result;
            result = null;

            result = ReadData(dataName);

            return result;
        }


        /// <summary>
        /// read raw data from dataName and encode to json format
        /// </summary>
        /// <param name="dataName"></param>
        /// <returns></returns>
        private byte[] ReadRaw(string dataName)
        {

            byte[] result;
            string dataType;
            result = null;
            dataType = GetDataType(dataName);
            if (dataType != null) {
                byte[] dataBuffer;
                dataBuffer = ReadData(dataName);

                if (dataBuffer != null)
                {
                    oldlclr.Codec codec;

                    codec = new oldlclr.Codec();
                    codec.Data = dataBuffer;
                    codec.DataType = dataType;
                    result = codec.Encode();
                }
            }
            return result;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataName"></param>
        /// <returns></returns>
        private string GetDataType(string dataName)
        {
            string result;
            result = DataType;
            if (result == null)
            {
                if (!IsStdIn(dataName))
                {
                    result = System.IO.Path.GetExtension(dataName);
                    
                }
            }
            return result;
        }




        /// <summary>
        /// program entry point
        /// </summary>
        /// <param name="args"></param>
        static int Main(string[] args)
        {
            int result;
            result = 0;

            Client client;
            client = new Client(System.AppDomain.CurrentDomain.FriendlyName);




            Gnu.Getopt.Getopt getOpt;
            IList<Gnu.Getopt.LongOpt> longOptions;
            longOptions = new List<Gnu.Getopt.LongOpt>();
            System.IO.StringWriter shortOption;
            shortOption = new System.IO.StringWriter();

            foreach (var optValue in client.Options)
            {
                Gnu.Getopt.LongOpt longOpt;
                longOpt = optValue.LongOpt;
                if (longOpt != null)
                {
                    longOptions.Add(longOpt);
                }
                shortOption.Write(optValue.ShortOption);
                if (optValue.NeedParameter)
                {
                    shortOption.Write(':');
                }
            }


            
            getOpt = new Gnu.Getopt.Getopt(client.ProgramName, args, shortOption.ToString(), longOptions.ToArray());


            int opt = getOpt.getopt();
            while (opt != -1)
            {

                if (client.CodeOptionHandler.ContainsKey(opt))
                {
                    result = client.CodeOptionHandler[opt](getOpt.Optarg);
                }
                else
                {
                    client.HandleHelp(null);
                    result = -1;
                }
                if (result != 0)
                {
                    break;
                }
                opt = getOpt.getopt();
            }
            if (result == 0)
            {
                client.Run();
            }
            return result;

        }
    }

    

}
