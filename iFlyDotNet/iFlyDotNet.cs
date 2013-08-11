using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using System.Threading;
using NAudio.Wave;

namespace iFlyDotNet
{
    enum ErrorCode
    {
        MSP_SUCCESS = 0,
        MSP_ERROR_FAIL = -1,
        MSP_ERROR_EXCEPTION = -2,

        /* General errors 10100(0x2774) */
        MSP_ERROR_GENERAL = 10100, 	/* 0x2774 */
        MSP_ERROR_OUT_OF_MEMORY = 10101, 	/* 0x2775 */
        MSP_ERROR_FILE_NOT_FOUND = 10102, 	/* 0x2776 */
        MSP_ERROR_NOT_SUPPORT = 10103, 	/* 0x2777 */
        MSP_ERROR_NOT_IMPLEMENT = 10104, 	/* 0x2778 */
        MSP_ERROR_ACCESS = 10105, 	/* 0x2779 */
        MSP_ERROR_INVALID_PARA = 10106, 	/* 0x277A */
        MSP_ERROR_INVALID_PARA_VALUE = 10107, 	/* 0x277B */
        MSP_ERROR_INVALID_HANDLE = 10108, 	/* 0x277C */
        MSP_ERROR_INVALID_DATA = 10109, 	/* 0x277D */
        MSP_ERROR_NO_LICENSE = 10110, 	/* 0x277E */
        MSP_ERROR_NOT_INIT = 10111, 	/* 0x277F */
        MSP_ERROR_NULL_HANDLE = 10112, 	/* 0x2780 */
        MSP_ERROR_OVERFLOW = 10113, 	/* 0x2781 */
        MSP_ERROR_TIME_OUT = 10114, 	/* 0x2782 */
        MSP_ERROR_OPEN_FILE = 10115, 	/* 0x2783 */
        MSP_ERROR_NOT_FOUND = 10116, 	/* 0x2784 */
        MSP_ERROR_NO_ENOUGH_BUFFER = 10117, 	/* 0x2785 */
        MSP_ERROR_NO_DATA = 10118, 	/* 0x2786 */
        MSP_ERROR_NO_MORE_DATA = 10119, 	/* 0x2787 */
        MSP_ERROR_SKIPPED = 10120, 	/* 0x2788 */
        MSP_ERROR_ALREADY_EXIST = 10121, 	/* 0x2789 */
        MSP_ERROR_LOAD_MODULE = 10122, 	/* 0x278A */
        MSP_ERROR_BUSY = 10123, 	/* 0x278B */
        MSP_ERROR_INVALID_CONFIG = 10124, 	/* 0x278C */
        MSP_ERROR_VERSION_CHECK = 10125, 	/* 0x278D */
        MSP_ERROR_CANCELED = 10126, 	/* 0x278E */
        MSP_ERROR_INVALID_MEDIA_TYPE = 10127, 	/* 0x278F */
        MSP_ERROR_CONFIG_INITIALIZE = 10128, 	/* 0x2790 */
        MSP_ERROR_CREATE_HANDLE = 10129, 	/* 0x2791 */
        MSP_ERROR_CODING_LIB_NOT_LOAD = 10130, 	/* 0x2792 */

        /* Error codes of network 10200(0x27D8)*/
        MSP_ERROR_NET_GENERAL = 10200, 	/* 0x27D8 */
        MSP_ERROR_NET_OPENSOCK = 10201, 	/* 0x27D9 */   /* Open socket */
        MSP_ERROR_NET_CONNECTSOCK = 10202, 	/* 0x27DA */   /* Connect socket */
        MSP_ERROR_NET_ACCEPTSOCK = 10203, 	/* 0x27DB */   /* Accept socket */
        MSP_ERROR_NET_SENDSOCK = 10204, 	/* 0x27DC */   /* Send socket data */
        MSP_ERROR_NET_RECVSOCK = 10205, 	/* 0x27DD */   /* Recv socket data */
        MSP_ERROR_NET_INVALIDSOCK = 10206, 	/* 0x27DE */   /* Invalid socket handle */
        MSP_ERROR_NET_BADADDRESS = 10207, 	/* 0x27EF */   /* Bad network address */
        MSP_ERROR_NET_BINDSEQUENCE = 10208, 	/* 0x27E0 */   /* Bind after listen/connect */
        MSP_ERROR_NET_NOTOPENSOCK = 10209, 	/* 0x27E1 */   /* Socket is not opened */
        MSP_ERROR_NET_NOTBIND = 10210, 	/* 0x27E2 */   /* Socket is not bind to an address */
        MSP_ERROR_NET_NOTLISTEN = 10211, 	/* 0x27E3 */   /* Socket is not listenning */
        MSP_ERROR_NET_CONNECTCLOSE = 10212, 	/* 0x27E4 */   /* The other side of connection is closed */
        MSP_ERROR_NET_NOTDGRAMSOCK = 10213, 	/* 0x27E5 */   /* The socket is not datagram type */

        /* Error codes of mssp message 10300(0x283C) */
        MSP_ERROR_MSG_GENERAL = 10300, 	/* 0x283C */
        MSP_ERROR_MSG_PARSE_ERROR = 10301, 	/* 0x283D */
        MSP_ERROR_MSG_BUILD_ERROR = 10302, 	/* 0x283E */
        MSP_ERROR_MSG_PARAM_ERROR = 10303, 	/* 0x283F */
        MSP_ERROR_MSG_CONTENT_EMPTY = 10304, 	/* 0x2840 */
        MSP_ERROR_MSG_INVALID_CONTENT_TYPE = 10305, 	/* 0x2841 */
        MSP_ERROR_MSG_INVALID_CONTENT_LENGTH = 10306, 	/* 0x2842 */
        MSP_ERROR_MSG_INVALID_CONTENT_ENCODE = 10307, 	/* 0x2843 */
        MSP_ERROR_MSG_INVALID_KEY = 10308, 	/* 0x2844 */
        MSP_ERROR_MSG_KEY_EMPTY = 10309, 	/* 0x2845 */
        MSP_ERROR_MSG_SESSION_ID_EMPTY = 10310, 	/* 0x2846 */
        MSP_ERROR_MSG_LOGIN_ID_EMPTY = 10311, 	/* 0x2847 */
        MSP_ERROR_MSG_SYNC_ID_EMPTY = 10312, 	/* 0x2848 */
        MSP_ERROR_MSG_APP_ID_EMPTY = 10313, 	/* 0x2849 */
        MSP_ERROR_MSG_EXTERN_ID_EMPTY = 10314, 	/* 0x284A */
        MSP_ERROR_MSG_INVALID_CMD = 10315, 	/* 0x284B */
        MSP_ERROR_MSG_INVALID_SUBJECT = 10316, 	/* 0x284C */
        MSP_ERROR_MSG_INVALID_VERSION = 10317, 	/* 0x284D */
        MSP_ERROR_MSG_NO_CMD = 10318, 	/* 0x284E */
        MSP_ERROR_MSG_NO_SUBJECT = 10319, 	/* 0x284F */
        MSP_ERROR_MSG_NO_VERSION = 10320, 	/* 0x2850 */
        MSP_ERROR_MSG_MSSP_EMPTY = 10321, 	/* 0x2851 */
        MSP_ERROR_MSG_NEW_RESPONSE = 10322, 	/* 0x2852 */
        MSP_ERROR_MSG_NEW_CONTENT = 10323, 	/* 0x2853 */
        MSP_ERROR_MSG_INVALID_SESSION_ID = 10324, 	/* 0x2854 */

        /* Error codes of DataBase 10400(0x28A0)*/
        MSP_ERROR_DB_GENERAL = 10400, 	/* 0x28A0 */
        MSP_ERROR_DB_EXCEPTION = 10401, 	/* 0x28A1 */
        MSP_ERROR_DB_NO_RESULT = 10402, 	/* 0x28A2 */
        MSP_ERROR_DB_INVALID_USER = 10403, 	/* 0x28A3 */
        MSP_ERROR_DB_INVALID_PWD = 10404, 	/* 0x28A4 */
        MSP_ERROR_DB_CONNECT = 10405, 	/* 0x28A5 */
        MSP_ERROR_DB_INVALID_SQL = 10406, 	/* 0x28A6 */
        MSP_ERROR_DB_INVALID_APPID = 10407,	/* 0x28A7 */

        /* Error codes of Resource 10500(0x2904)*/
        MSP_ERROR_RES_GENERAL = 10500, 	/* 0x2904 */
        MSP_ERROR_RES_LOAD = 10501, 	/* 0x2905 */   /* Load resource */
        MSP_ERROR_RES_FREE = 10502, 	/* 0x2906 */   /* Free resource */
        MSP_ERROR_RES_MISSING = 10503, 	/* 0x2907 */   /* Resource File Missing */
        MSP_ERROR_RES_INVALID_NAME = 10504, 	/* 0x2908 */   /* Invalid resource file name */
        MSP_ERROR_RES_INVALID_ID = 10505, 	/* 0x2909 */   /* Invalid resource ID */
        MSP_ERROR_RES_INVALID_IMG = 10506, 	/* 0x290A */   /* Invalid resource image pointer */
        MSP_ERROR_RES_WRITE = 10507, 	/* 0x290B */   /* Write read-only resource */
        MSP_ERROR_RES_LEAK = 10508, 	/* 0x290C */   /* Resource leak out */
        MSP_ERROR_RES_HEAD = 10509, 	/* 0x290D */   /* Resource head currupt */
        MSP_ERROR_RES_DATA = 10510, 	/* 0x290E */   /* Resource data currupt */
        MSP_ERROR_RES_SKIP = 10511, 	/* 0x290F */   /* Resource file skipped */

        /* Error codes of TTS 10600(0x2968)*/
        MSP_ERROR_TTS_GENERAL = 10600, 	/* 0x2968 */
        MSP_ERROR_TTS_TEXTEND = 10601, 	/* 0x2969 */  /* Meet text end */
        MSP_ERROR_TTS_TEXT_EMPTY = 10602, 	/* 0x296A */  /* no synth text */

        /* Error codes of Recognizer 10700(0x29CC) */
        MSP_ERROR_REC_GENERAL = 10700, 	/* 0x29CC */
        MSP_ERROR_REC_INACTIVE = 10701, 	/* 0x29CD */
        MSP_ERROR_REC_GRAMMAR_ERROR = 10702, 	/* 0x29CE */
        MSP_ERROR_REC_NO_ACTIVE_GRAMMARS = 10703, 	/* 0x29CF */
        MSP_ERROR_REC_DUPLICATE_GRAMMAR = 10704, 	/* 0x29D0 */
        MSP_ERROR_REC_INVALID_MEDIA_TYPE = 10705, 	/* 0x29D1 */
        MSP_ERROR_REC_INVALID_LANGUAGE = 10706, 	/* 0x29D2 */
        MSP_ERROR_REC_URI_NOT_FOUND = 10707, 	/* 0x29D3 */
        MSP_ERROR_REC_URI_TIMEOUT = 10708, 	/* 0x29D4 */
        MSP_ERROR_REC_URI_FETCH_ERROR = 10709, 	/* 0x29D5 */

        /* Error codes of Speech Detector 10800(0x2A30) */
        MSP_ERROR_EP_GENERAL = 10800, 	/* 0x2A30 */
        MSP_ERROR_EP_NO_SESSION_NAME = 10801, 	/* 0x2A31 */
        MSP_ERROR_EP_INACTIVE = 10802, 	/* 0x2A32 */
        MSP_ERROR_EP_INITIALIZED = 10803, 	/* 0x2A33 */

        /* Error codes of TUV */
        MSP_ERROR_TUV_GENERAL = 10900, 	/* 0x2A94 */
        MSP_ERROR_TUV_GETHIDPARAM = 10901, 	/* 0x2A95 */   /* Get Busin Param huanid*/
        MSP_ERROR_TUV_TOKEN = 10902, 	/* 0x2A96 */   /* Get Token */
        MSP_ERROR_TUV_CFGFILE = 10903, 	/* 0x2A97 */   /* Open cfg file */
        MSP_ERROR_TUV_RECV_CONTENT = 10904, 	/* 0x2A98 */   /* received content is error */
        MSP_ERROR_TUV_VERFAIL = 10905, 	/* 0x2A99 */   /* Verify failure */

        /* Error codes of IMTV */
        MSP_ERROR_IMTV_SUCCESS = 11000, 	/* 0x2AF8 */   /* 成功 */
        MSP_ERROR_IMTV_NO_LICENSE = 11001, 	/* 0x2AF9 */   /* 试用次数结束，用户需要付费 */
        MSP_ERROR_IMTV_SESSIONID_INVALID = 11002, 	/* 0x2AFA */   /* SessionId失效，需要重新登录通行证 */
        MSP_ERROR_IMTV_SESSIONID_ERROR = 11003, 	/* 0x2AFB */   /* SessionId为空，或者非法 */
        MSP_ERROR_IMTV_UNLOGIN = 11004, 	/* 0x2AFC */   /* 未登录通行证 */
        MSP_ERROR_IMTV_SYSTEM_ERROR = 11005, 	/* 0x2AFD */   /* 系统错误 */

        /* Error codes of HCR */
        MSP_ERROR_HCR_GENERAL = 11100,
        MSP_ERROR_HCR_RESOURCE_NOT_EXIST = 11101,

        /* Error codes of http 12000(0x2EE0) */
        MSP_ERROR_HTTP_BASE = 12000,	/* 0x2EE0 */

        /*Error codes of ISV */
        MSP_ERROR_ISV_NO_USER = 13000,	/* 32C8 */    /* the user doesn't exist */
    }

    #region TTS枚举常量
    /// <summary>
    /// vol参数的枚举常量
    /// </summary>
    public enum enuVol
    {
        x_soft,
        soft,
        medium,
        loud,
        x_loud
    }

    /// <summary>
    /// speed语速参数的枚举常量
    /// </summary>
    public enum enuSpeed
    {
        x_slow,
        slow,
        medium,
        fast,
        x_fast
    }
    /// <summary>
    /// speeker朗读者枚举常量
    /// </summary>
    public enum enuSpeeker
    {
        小燕_青年女声_中英文_普通话 = 0,
        小宇_青年男声_中英文_普通话,
        凯瑟琳_青年女声_英语,
        亨利_青年男声_英语,
        玛丽_青年女声_英语,
        小研_青年女声_中英文_普通话,
        小琪_青年女声_中英文_普通话,
        小峰_青年男声_中英文_普通话,
        小梅_青年女声_中英文_粤语,
        小莉_青年女声_中英文_台普,
        小蓉_青年女声_汉语_四川话,
        小芸_青年女声_汉语_东北话,
        小坤_青年男声_汉语_河南话,
        小强_青年男声_汉语_湖南话,
        小莹_青年女声_汉语_陕西话,
        小新_童年男声_汉语_普通话,
        楠楠_童年女声_汉语_普通话,
        老孙_老年男声_汉语_普通话,
        玛丽安_青年女声_法语,
        古丽_青年女声_维吾尔语,
        阿拉本_青年女声_俄罗斯语,
        加芙列拉_青年女声_西班牙语,
        艾伯哈_青年女声_印地语,
        小云_青年女声_越南语
    }

    public enum SynthStatus
    {
        TTS_FLAG_STILL_HAVE_DATA = 1,
        TTS_FLAG_DATA_END,
        TTS_FLAG_CMD_CANCELED
    }
    #endregion

    #region ISR枚举常量
    public enum AudioStatus
    {
        ISR_AUDIO_SAMPLE_INIT = 0x00,
        ISR_AUDIO_SAMPLE_FIRST = 0x01,
        ISR_AUDIO_SAMPLE_CONTINUE = 0x02,
        ISR_AUDIO_SAMPLE_LAST = 0x04,
        ISR_AUDIO_SAMPLE_SUPPRESSED = 0x08,
        ISR_AUDIO_SAMPLE_LOST = 0x10,
        ISR_AUDIO_SAMPLE_NEW_CHUNK = 0x20,
        ISR_AUDIO_SAMPLE_END_CHUNK = 0x40,

        ISR_AUDIO_SAMPLE_VALIDBITS = 0x7f /* to validate the value of sample->status */
    }

    public enum EpStatus
    {
        ISR_EP_NULL = -1,
        ISR_EP_LOOKING_FOR_SPEECH = 0,          ///还没有检测到音频的前端点
        ISR_EP_IN_SPEECH = 1,                   ///已经检测到了音频前端点，正在进行正常的音频处理。
        ISR_EP_AFTER_SPEECH = 3,                ///检测到音频的后端点，后继的音频会被MSC忽略。
        ISR_EP_TIMEOUT = 4,                     ///超时
        ISR_EP_ERROR = 5,                       ///出现错误
        ISR_EP_MAX_SPEECH = 6                   ///音频过大
    }

    public enum RecogStatus
    {
        ISR_REC_NULL = -1,
        ISR_REC_STATUS_SUCCESS = 0,             ///识别成功，此时用户可以调用QISRGetResult来获取（部分）结果。
        ISR_REC_STATUS_NO_MATCH = 1,            ///识别结束，没有识别结果
        ISR_REC_STATUS_INCOMPLETE = 2,          ///正在识别中
        ISR_REC_STATUS_NON_SPEECH_DETECTED = 3, ///保留
        ISR_REC_STATUS_SPEECH_DETECTED = 4,     ///发现有效音频
        ISR_REC_STATUS_SPEECH_COMPLETE = 5,     ///识别结束
        ISR_REC_STATUS_MAX_CPU_TIME = 6,        ///保留
        ISR_REC_STATUS_MAX_SPEECH = 7,          ///保留
        ISR_REC_STATUS_STOPPED = 8,             ///保留
        ISR_REC_STATUS_REJECTED = 9,            ///保留
        ISR_REC_STATUS_NO_SPEECH_FOUND = 10     ///没有发现音频
    }
    #endregion

    public class iFlyTTS
    {
        /// <summary>
        /// 事件消息类，类中包含了合成文本的最大值和当前已经合成的值
        /// </summary>
        public class FinishedEventArgs : EventArgs
        {
            public readonly int Max;
            public readonly int Cur;
            public FinishedEventArgs(int max, int cur)
            {
                Max = max;
                Cur = cur;
            }
        }
        /// <summary>
        /// 已合成文本的行数，事件
        /// </summary>
        public event EventHandler<FinishedEventArgs> FinishedLine;
        /// <summary>
        /// 当前文本行中已合成的文本数，事件
        /// </summary>
        public event EventHandler<FinishedEventArgs> FinishedText;
        /// <summary>
        /// 引入TTSDll函数的类
        /// </summary>
        private class TTSDll
        {
            #region TTS dll import

            [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
            public static extern int QTTSInit(string configs);

            [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
            public static extern IntPtr QTTSSessionBegin(string _params, ref int errorCode);

            [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
            public static extern int QTTSTextPut(string sessionID, string textString, uint textLen, string _params);

            [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
            public static extern IntPtr QTTSAudioGet(string sessionID, ref int audioLen, ref SynthStatus synthStatus, ref int errorCode);

            [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
            public static extern IntPtr QTTSAudioInfo(string sessionID);

            [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
            public static extern int QTTSSessionEnd(string sessionID, string hints);

            [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
            public static extern int QTTSGetParam(string sessionID, string paramName, string paramValue, ref uint valueLen);

            [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
            public static extern int QTTSFini();
            #endregion
        }
        private string sessionID;
        /// <summary>
        /// 合成音频的采样频率，8000、16000、44100等
        /// </summary>
        public int rate { get; set; }

        /// <summary>
        /// 讯飞的AppID
        /// </summary>
        /// public string appid { get; set; }

        private string _speed;
        /// <summary>
        /// 语速
        /// </summary>
        public enuSpeed speed
        {
            get { return (enuSpeed)Enum.Parse(typeof(enuVol), _speed); }
            set { _speed = value.ToString("G").Replace('_', '-'); }
        }

        private string _vol;
        /// <summary>
        /// 音量
        /// </summary>
        public enuVol vol
        {
            get { return (enuVol)Enum.Parse(typeof(enuVol), _vol); }
            set { _vol = value.ToString("G").Replace('_', '-'); }
        }
        /// <summary>
        /// 最大音频长度
        /// </summary>
        public long max { get; set; }

        public enuSpeeker speeker
        {
            get { return speeker; }
            set { DSpeeker.TryGetValue(value, out _speeker); }
        }

        private string _speeker;
        
        private Dictionary<enuSpeeker, string> DSpeeker = new Dictionary<enuSpeeker, string>();

        private Dictionary<string, string> DSpeekerName = new Dictionary<string, string>();

        private byte[] buffting;
        /// <summary>
        /// 构造函数，初始化引擎
        /// </summary>
        /// <param name="configs">初始化引擎参数</param>
        /// <param name="szParams">开始会话用参数</param>
        public iFlyTTS(string configs)
        {
            DSpeeker.Add(enuSpeeker.小燕_青年女声_中英文_普通话, "ent=intp65,vcn=xiaoyan");
            DSpeeker.Add(enuSpeeker.小宇_青年男声_中英文_普通话, "ent=intp65,vcn=xiaoyu");
            DSpeeker.Add(enuSpeeker.凯瑟琳_青年女声_英语, "ent=intp65_en,vcn=Catherine");
            DSpeeker.Add(enuSpeeker.亨利_青年男声_英语, "ent=intp65_en,vcn=henry");
            DSpeeker.Add(enuSpeeker.玛丽_青年女声_英语, "ent=vivi21,vcn=vimary");
            DSpeeker.Add(enuSpeeker.小研_青年女声_中英文_普通话, "ent=vivi21,vcn=vixy");
            DSpeeker.Add(enuSpeeker.小琪_青年女声_中英文_普通话, "ent=vivi21,vcn=vixq");
            DSpeeker.Add(enuSpeeker.小峰_青年男声_中英文_普通话, "ent=vivi21,vcn=vixf");
            DSpeeker.Add(enuSpeeker.小梅_青年女声_中英文_粤语, "ent=vivi21,vcn=vixm");
            DSpeeker.Add(enuSpeeker.小莉_青年女声_中英文_台普, "ent=vivi21,vcn=vixl");
            DSpeeker.Add(enuSpeeker.小蓉_青年女声_汉语_四川话, "ent=vivi21,vcn=vixr");
            DSpeeker.Add(enuSpeeker.小芸_青年女声_汉语_东北话, "ent=vivi21,vcn=vixyun");
            DSpeeker.Add(enuSpeeker.小坤_青年男声_汉语_河南话, "ent=vivi21,vcn=vixk");
            DSpeeker.Add(enuSpeeker.小强_青年男声_汉语_湖南话, "ent=vivi21,vcn=vixqa");
            DSpeeker.Add(enuSpeeker.小莹_青年女声_汉语_陕西话, "ent=vivi21,vcn=vixying");
            DSpeeker.Add(enuSpeeker.小新_童年男声_汉语_普通话, "ent=vivi21,vcn=vixx");
            DSpeeker.Add(enuSpeeker.楠楠_童年女声_汉语_普通话, "ent=vivi21,vcn=vinn");
            DSpeeker.Add(enuSpeeker.老孙_老年男声_汉语_普通话, "ent=vivi21,vcn=vils");
            DSpeeker.Add(enuSpeeker.玛丽安_青年女声_法语, "ent=mtts,vcn=Mariane");
            DSpeeker.Add(enuSpeeker.古丽_青年女声_维吾尔语, "ent=mtts,vcn=Guli");
            DSpeeker.Add(enuSpeeker.阿拉本_青年女声_俄罗斯语, "ent=mtts,vcn=Allabent");
            DSpeeker.Add(enuSpeeker.加芙列拉_青年女声_西班牙语, "ent=mtts,vcn=Gabriela");
            DSpeeker.Add(enuSpeeker.艾伯哈_青年女声_印地语, "ent=mtts,vcn=Abha");
            DSpeeker.Add(enuSpeeker.小云_青年女声_越南语, "ent=mtts,vcn=XiaoYun");

            DSpeekerName.Add("xiaoyan", "ent=intp65,vcn=xiaoyan");
            DSpeekerName.Add("xiaoyu", "ent=intp65,vcn=xiaoyu");
            DSpeekerName.Add("catherine", "ent=intp65_en,vcn=Catherine");
            DSpeekerName.Add("henry", "ent=intp65_en,vcn=henry");
            DSpeekerName.Add("mary", "ent=vivi21,vcn=vimary");
            DSpeekerName.Add("xy", "ent=vivi21,vcn=vixy");
            DSpeekerName.Add("xq", "ent=vivi21,vcn=vixq");
            DSpeekerName.Add("xf", "ent=vivi21,vcn=vixf");
            DSpeekerName.Add("xm", "ent=vivi21,vcn=vixm");
            DSpeekerName.Add("xl", "ent=vivi21,vcn=vixl");
            DSpeekerName.Add("xr", "ent=vivi21,vcn=vixr");
            DSpeekerName.Add("xyun", "ent=vivi21,vcn=vixyun");
            DSpeekerName.Add("xk", "ent=vivi21,vcn=vixk");
            DSpeekerName.Add("xqa", "ent=vivi21,vcn=vixqa");
            DSpeekerName.Add("xying", "ent=vivi21,vcn=vixying");
            DSpeekerName.Add("xx", "ent=vivi21,vcn=vixx");
            DSpeekerName.Add("nn", "ent=vivi21,vcn=vinn");
            DSpeekerName.Add("ls", "ent=vivi21,vcn=vils");
            DSpeekerName.Add("Mariane", "ent=mtts,vcn=Mariane");
            DSpeekerName.Add("Guli", "ent=mtts,vcn=Guli");
            DSpeekerName.Add("Allabent", "ent=mtts,vcn=Allabent");
            DSpeekerName.Add("Gabriela", "ent=mtts,vcn=Gabriela");
            DSpeekerName.Add("Abha", "ent=mtts,vcn=Abha");
            DSpeekerName.Add("XiaoYun", "ent=mtts,vcn=XiaoYun");

            buffting = iFlyResource.ding;

            int ret = TTSDll.QTTSInit(configs);
            if (ret != 0) throw new Exception("初始化TTS引擎错误，错误代码：" + ret);
        }
        /// <summary>
        /// MultiSpeek的异步版本,通过委托实现
        /// </summary>
        /// <param name="SpeekText"></param>
        /// <param name="outWaveFlie"></param>
        public void MultiSpeekSync(string SpeekText, string outWaveFlie = null)
        {
            Action<string, string> dSpeek = MultiSpeek;
            dSpeek.BeginInvoke(SpeekText, outWaveFlie, null, null);//在另一线程中调用MultiSpeek实现异步操作
        }
        /// <summary>
        /// 把文字转化为声音,多路配置，多种语音
        /// </summary>
        /// <param name="speekText">要转化成语音的文字</param>
        /// <param name="outWaveFlie">把声音转为文件，默认为不生产wave文件</param>
        public void MultiSpeek(string SpeekText, string outWaveFlie = null)
        {
            System.Console.WriteLine("开始，now=" + System.DateTime.Now);
            MemoryStream mStream = new MemoryStream();
            try
            {
                string[] SpeekTexts = System.Text.RegularExpressions.Regex.Split(SpeekText, "\r\n");
                mStream.Write(new byte[44], 0, 44);
                for (int i = 0; i < SpeekTexts.Length; i++)
                {
                    FinishedEventArgs e = new FinishedEventArgs(SpeekTexts.Length, i);
                    if (FinishedLine != null)
                        FinishedLine(this, e);
                    string ThisStr = SpeekTexts[i];
                    ThisStr = ThisStr.Trim();               //去除前后的空白
                    if (ThisStr == "") continue;            //空段的处理
                    int pos = ThisStr.IndexOf('#');         //#在段中的位置
                    if (pos > 0)
                    {
                        //设定了讲话者时用指定的讲话者说
                        DSpeekerName.TryGetValue(ThisStr.Substring(0, pos).ToLower(), out _speeker);
                        speek(ThisStr.Substring(pos + 1, ThisStr.Length - pos - 1), ref mStream);
                    }
                    else
                    {
                        if (ThisStr.Length < 4)
                        {//没有指定讲话者文本长度小于4
                            speek(ThisStr, ref mStream);
                            continue;
                        }
                        if (ThisStr.Substring(0, 4).ToLower() == "stop")
                        {//暂停一段时间
                            int s = Convert.ToInt32(ThisStr.Substring(4, ThisStr.Length - 4));
                            byte[] bs = new byte[32000];
                            for (int j = 0; j < s; j++)
                            {
                                mStream.Write(new byte[32000], 0, 32000);
                            }
                        }
                        else if (ThisStr.Substring(0, 4).ToLower() == "ting")
                        {//插入叮铃声
                            mStream.Write(buffting, 0, buffting.Length);
                        }
                        else
                        {//没有指定讲话者文本长度大于等于4
                            speek(ThisStr, ref mStream);
                        }
                    }
                }
                int ret = TTSDll.QTTSFini();
                if (ret != 0) throw new Exception("逆初始化TTS引擎错误，错误代码：" + ((ErrorCode)ret).ToString("G"));

                WAVE_Header header = getWave_Header((int)mStream.Length - 44);     //创建wav文件头
                byte[] headerByte = StructToBytes(header);                         //把文件头结构转化为字节数组                      //写入文件头
                mStream.Position = 0;                                                        //定位到文件头
                mStream.Write(headerByte, 0, headerByte.Length);                             //写入文件头
                //mStream.Position = 0;
                //System.Media.SoundPlayer pl = new System.Media.SoundPlayer(mStream);
                //pl.Stop();
                //pl.Play();
                if (outWaveFlie != null)
                {
                    FileStream ofs = new FileStream(outWaveFlie, FileMode.Create);
                    mStream.WriteTo(ofs);
                    ofs.Close();
                    ofs = null;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
            finally
            {
                int ret = TTSDll.QTTSFini();
                mStream.Close();
                mStream = null;
            }
            System.Console.WriteLine("完成，now=" + System.DateTime.Now);
        }

        /// <summary>
        /// 把文本转换成声音，写入指定的内存流
        /// </summary>
        /// <param name="SpeekText">要转化成语音的文字</param>
        /// <param name="mStream">合成结果输出的音频流</param>
        private void speek(string SpeekText, ref MemoryStream mStream)
        {
            if (SpeekText == "" || _speed == "" || _vol == "" || _speeker == "") return;
            string szParams = "ssm=1," + _speeker + ",spd=" + _speed + ",aue=speex-wb;7,vol=" + _vol + ",auf=audio/L16;rate=16000";
            int ret = 0;
            try
            {
                sessionID = Ptr2Str(TTSDll.QTTSSessionBegin(szParams, ref ret));
                if (ret != 0) throw new Exception("初始化TTS引会话错误，错误代码：" + ret);

                ret = TTSDll.QTTSTextPut(sessionID, SpeekText, (uint)Encoding.Default.GetByteCount(SpeekText), string.Empty);
                if (ret != 0) throw new Exception("向服务器发送数据，错误代码：" + ret);
                IntPtr audio_data;
                int audio_len = 0;
                SynthStatus synth_status = SynthStatus.TTS_FLAG_STILL_HAVE_DATA;

                MemoryStream fs = mStream;
                int pos = 0; 
                int oldPos = 0;
                byte[] tmpArray = Encoding.Default.GetBytes(SpeekText);
                while (synth_status == SynthStatus.TTS_FLAG_STILL_HAVE_DATA)
                {
                    audio_data = TTSDll.QTTSAudioGet(sessionID, ref audio_len, ref synth_status, ref ret);
                    string posStr = Ptr2Str(TTSDll.QTTSAudioInfo(sessionID));
                    pos = Convert.ToInt32(posStr.Substring(4));
                    FinishedEventArgs e = new FinishedEventArgs(tmpArray.Length, pos);
                    if (FinishedText != null && pos - oldPos != 0)
                    {
                        FinishedText(this, e);
                        System.Console.WriteLine(posStr + ":" + Encoding.Default.GetString(tmpArray, oldPos, pos - oldPos));
                    }
                    oldPos = pos;
                    if (ret != 0) break;
                    byte[] data = new byte[audio_len];
                    if (audio_len > 0) Marshal.Copy(audio_data, data, 0, audio_len);
                    fs.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            finally
            {
                ret = TTSDll.QTTSSessionEnd(sessionID, "");
                if (ret != 0) throw new Exception("结束TTS会话错误，错误代码：" + ret);
            }
        }

        /// <summary>
        /// 把文字转化为声音,单路配置，一种语音
        /// </summary>
        /// <param name="speekText">要转化成语音的文字</param>
        /// <param name="outWaveFlie">把声音转为文件，默认为不生产wave文件</param>
        private void speek(string speekText, string outWaveFlie = null)
        {
            if (speekText == "" || _speed == "" || _vol == "" || _speeker == "") return;
            DSpeeker.TryGetValue(speeker, out _speeker);
            string szParams = "ssm=1," + _speeker + ",spd=" + _speed + ",aue=speex-wb;7,vol=" + _vol + ",auf=audio/L16;rate=16000";
            int ret = 0;
            try
            {
                sessionID = Ptr2Str(TTSDll.QTTSSessionBegin(szParams, ref ret));
                if (ret != 0) throw new Exception("初始化TTS引会话错误，错误代码：" + ret);

                ret = TTSDll.QTTSTextPut(sessionID, speekText, (uint)Encoding.Default.GetByteCount(speekText), string.Empty);
                if (ret != 0) throw new Exception("向服务器发送数据，错误代码：" + ret);
                IntPtr audio_data;
                int audio_len = 0;
                SynthStatus synth_status = SynthStatus.TTS_FLAG_STILL_HAVE_DATA;

                MemoryStream fs = new MemoryStream();
                fs.Write(new byte[44], 0, 44);                              //写44字节的空文件头

                while (synth_status == SynthStatus.TTS_FLAG_STILL_HAVE_DATA)
                {
                    audio_data = TTSDll.QTTSAudioGet(sessionID, ref audio_len, ref synth_status, ref ret);
                    if (ret != 0) break;
                    byte[] data = new byte[audio_len];
                    if (audio_len > 0) Marshal.Copy(audio_data, data, 0, audio_len);
                    fs.Write(data, 0, data.Length);
                }

                WAVE_Header header = getWave_Header((int)fs.Length - 44);     //创建wav文件头
                byte[] headerByte = StructToBytes(header);                         //把文件头结构转化为字节数组                      //写入文件头
                fs.Position = 0;                                                        //定位到文件头
                fs.Write(headerByte, 0, headerByte.Length);                             //写入文件头

                fs.Position = 0;
                System.Media.SoundPlayer pl = new System.Media.SoundPlayer(fs);
                pl.Stop();
                pl.Play();
                if (outWaveFlie != null)
                {
                    FileStream ofs = new FileStream(outWaveFlie, FileMode.Create);
                    fs.WriteTo(ofs);
                    fs.Close();
                    ofs.Close();
                    fs = null;
                    ofs = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            finally
            {
                ret = TTSDll.QTTSSessionEnd(sessionID, "");
                if (ret != 0) throw new Exception("结束TTS会话错误，错误代码：" + ret);
            }
        }

        /// <summary>
        /// wave文件头
        /// </summary>
        private struct WAVE_Header
        {
            public int RIFF_ID;           //4 byte , 'RIFF'
            public int File_Size;         //4 byte , 文件长度
            public int RIFF_Type;         //4 byte , 'WAVE'

            public int FMT_ID;            //4 byte , 'fmt'
            public int FMT_Size;          //4 byte , 数值为16或18，18则最后又附加信息
            public short FMT_Tag;          //2 byte , 编码方式，一般为0x0001
            public ushort FMT_Channel;     //2 byte , 声道数目，1--单声道；2--双声道
            public int FMT_SamplesPerSec;//4 byte , 采样频率
            public int AvgBytesPerSec;   //4 byte , 每秒所需字节数,记录每秒的数据量
            public ushort BlockAlign;      //2 byte , 数据块对齐单位(每个采样需要的字节数)
            public ushort BitsPerSample;   //2 byte , 每个采样需要的bit数

            public int DATA_ID;           //4 byte , 'data'
            public int DATA_Size;         //4 byte , 
        }

        /// <summary>
        /// 根据数据段的长度，生产文件头
        /// </summary>
        /// <param name="data_len">音频数据长度</param>
        /// <returns>返回wav文件头结构体</returns>
        WAVE_Header getWave_Header(int data_len)
        {
            WAVE_Header wav_Header = new WAVE_Header();
            wav_Header.RIFF_ID = 0x46464952;        //字符RIFF
            wav_Header.File_Size = data_len + 36;
            wav_Header.RIFF_Type = 0x45564157;      //字符WAVE

            wav_Header.FMT_ID = 0x20746D66;         //字符fmt
            wav_Header.FMT_Size = 16;
            wav_Header.FMT_Tag = 0x0001;
            wav_Header.FMT_Channel = 1;             //单声道
            wav_Header.FMT_SamplesPerSec = 16000;   //采样频率
            wav_Header.AvgBytesPerSec = 32000;      //每秒所需字节数
            wav_Header.BlockAlign = 2;              //每个采样1个字节
            wav_Header.BitsPerSample = 16;           //每个采样8bit

            wav_Header.DATA_ID = 0x61746164;        //字符data
            wav_Header.DATA_Size = data_len;

            return wav_Header;
        }

        /// <summary>
        /// 把结构体转化为字节序列
        /// </summary>
        /// <param name="structure">被转化的结构体</param>
        /// <returns>返回字节序列</returns>
        Byte[] StructToBytes(Object structure)
        {
            Int32 size = Marshal.SizeOf(structure);
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(structure, buffer, false);
                Byte[] bytes = new Byte[size];
                Marshal.Copy(buffer, bytes, 0, size);
                return bytes;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        /// <summary>
        /// 指针转字符串
        /// </summary>
        /// <param name="p">指向非托管代码字符串的指针</param>
        /// <returns>返回指针指向的字符串</returns>
        private string Ptr2Str(IntPtr p)
        {
            List<byte> lb = new List<byte>();
            while (Marshal.ReadByte(p) != 0)
            {
                lb.Add(Marshal.ReadByte(p));
                p = p + 1;
            }
            byte[] bs = lb.ToArray();
            return Encoding.Default.GetString(lb.ToArray());
        }
    }

    public class iFlyISR
    {
        public class ASRDll
        {
            #region ISR dllimport

            [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
            public static extern int QISRInit(string configs);

            [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
            public static extern IntPtr QISRSessionBegin(string grammarList, string _params, ref int errorCode);

            [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
            public static extern int QISRGrammarActivate(string sessionID, string grammar, string type, int weight);

            [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
            public static extern int QISRAudioWrite(string sessionID, IntPtr waveData, uint waveLen, AudioStatus audioStatus, ref  EpStatus epStatus, ref RecogStatus recogStatus);

            [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
            public static extern IntPtr QISRGetResult(string sessionID, ref RecogStatus rsltStatus, int waitTime, ref int errorCode);

            [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
            public static extern int QISRSessionEnd(string sessionID, string hints);

            [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
            public static extern int QISRGetParam(string sessionID, string paramName, string paramValue, ref uint valueLen);

            [DllImport("msc.dll", CallingConvention = CallingConvention.StdCall)]
            public static extern int QISRFini();
            #endregion
        }
        /// <summary>
        /// 有识别数据返回的事件参数，包含了识别的文本结果
        /// </summary>
        public class DataArrivedEventArgs : EventArgs
        {
            public string result;
            public DataArrivedEventArgs(string rs)
            {
                result = rs;
            }
        }

        /// <summary>
        /// 识别数据返回的事件
        /// </summary>
        public event EventHandler<DataArrivedEventArgs> DataArrived;

        /// <summary>
        /// 识别过程结束的事件
        /// </summary>
        public event EventHandler ISREnd;
        
        //string c1 = "server_url=dev.voicecloud.cn,appid=4e7bf06d,timeout=10000";
        //string c2 = "sub=iat,ssm=1,auf=audio/L16;rate=16000,aue=speex,ent=sms16k,rst=plain";
        public string appid;
        public int rate;
        public string auf;

        private WaveInStream wis;
        private const int BUFFER_NUM = 1024 * 20;
        private const int waitTime = 1000;
        private string sess_id;
        /// <summary>
        /// 构造函数，初始化引擎，开Session
        /// </summary>
        /// <param name="c1">初始化引擎的参数</param>
        /// <param name="c2">开session的参数</param>
        public iFlyISR(string c1, string c2, out string sess_id)
        {
            wis = new WaveInStream(0, new WaveFormat(16000, 16, 1), null);
            wis.DataAvailable += new EventHandler<WaveInEventArgs>(wis_DataAvailable);

            int ret = 0;
            ///引擎初始化，只需初始化一次
            ret = ASRDll.QISRInit(c1);
            if (ret != 0) throw new Exception("QISP初始化失败，错误代码:" + ((ErrorCode)ret).ToString("G"));
            ///第二个参数为传递的参数，使用会话模式，使用speex编解码，使用16k16bit的音频数据
            ///第三个参数为返回码
            string param = c2;
            sess_id = Ptr2Str(ASRDll.QISRSessionBegin(string.Empty, param, ref ret));
            if (ret != 0) throw new Exception("QISRSessionBegin失败，错误代码:" + ((ErrorCode)ret).ToString("G"));
        }

        //public void run_iat(ref string src_wav_filename, ref string des_text_filename, ref string param)
        //{
        //    string sess_id;
        //    bool error = false;
        //    int ret = 0;
        //    int i = 0;
        //    FileStream fp = new FileStream(src_wav_filename, FileMode.Open);
        //    const int BUFFER_NUM = 1024 * 4;
        //    const int AMR_HEAD_SIZE = 6;
        //    byte[] buff = new byte[BUFFER_NUM];
        //    int len;
        //    int status = 0x02, ep_status = -1, rec_status = -1, rslt_status = -1;
        //    sess_id = Ptr2Str(ASRDll.QISRSessionBegin(null, param, ref ret));
        //    char[] param_value = new char[32];
        //    param_value = null;
        //    int value_len = 32;	//字符串长度或buffer长度
        //    int volume = 0;//音量数值
        //    ///开始向服务器发送音频数据
        //    ret = ASRDll.QISRAudioWrite(sess_id, buff, len, status, ref ep_status, ref rec_status);
        //}

        /// <summary>
        /// 执行语音识别的异步方法
        /// </summary>
        /// <param name="inFile">音频文件，pcm无文件头，采样率16k，数据16位，单声道</param>
        /// <param name="outFile">输出识别结果到文件</param>
        public void Audio2TxtAsync(string inFile, string outFile = null)
        {
            Action<string, string> dlt = Audio2Txt;
            dlt.BeginInvoke(inFile, outFile, null, null);
        }

        /// <summary>
        /// 进行声音识别
        /// </summary>
        /// <param name="inFile">音频文件，pcm无文件头，采样率16k，数据16位，单声道</param>
        /// <param name="outFile">输出识别结果到文件</param>
        public void Audio2Txt(string inFile, string outFile = null)
        {
            int ret = 0;
            string result = "";
            try
            {
                ///模拟录音，输入音频
                if (!File.Exists(inFile)) throw new Exception("文件" + inFile + "不存在！");
                if (inFile.Substring(inFile.Length - 3, 3).ToUpper() != "WAV" && inFile.Substring(inFile.Length - 3, 3).ToUpper() != "PCM")
                    throw new Exception("音频文件格式不对！");
                FileStream fp = new FileStream(inFile, FileMode.Open);
                if (inFile.Substring(inFile.Length - 3, 3).ToUpper() == "WAV") fp.Position = 44;
                byte[] buff = new byte[BUFFER_NUM];
                IntPtr bp = Marshal.AllocHGlobal(BUFFER_NUM);
                int len;
                AudioStatus status = AudioStatus.ISR_AUDIO_SAMPLE_CONTINUE;
                EpStatus ep_status = EpStatus.ISR_EP_NULL;
                RecogStatus rec_status = RecogStatus.ISR_REC_NULL;
                RecogStatus rslt_status = RecogStatus.ISR_REC_NULL;
                ///ep_status        端点检测（End-point detected）器所处的状态
                ///rec_status       识别器所处的状态
                ///rslt_status      识别器所处的状态
                while (fp.Position != fp.Length)
                {
                    len = fp.Read(buff, 0, BUFFER_NUM);
                    Marshal.Copy(buff, 0, bp, buff.Length);
                    ///开始向服务器发送音频数据
                    ret = ASRDll.QISRAudioWrite(sess_id, bp, (uint)len, status, ref ep_status, ref rec_status);
                    if (ret != 0)
                    {
                        fp.Close();
                        throw new Exception("QISRAudioWrite err,errCode=" + ((ErrorCode)ret).ToString("G"));
                    }
                    ///服务器返回部分结果
                    if (rec_status == RecogStatus.ISR_REC_STATUS_SUCCESS)
                    {
                        IntPtr p = ASRDll.QISRGetResult(sess_id, ref rslt_status, waitTime, ref ret);
                        if (p != IntPtr.Zero)
                        {
                            string tmp = Ptr2Str(p);
                            DataArrived(this, new DataArrivedEventArgs(tmp));
                            result += tmp;
                            System.Console.WriteLine("返回部分结果！:" + tmp);
                        }
                    }
                    System.Threading.Thread.Sleep(500);
                }
                fp.Close();

                ///最后一块数据
                status = AudioStatus.ISR_AUDIO_SAMPLE_LAST;

                ret = ASRDll.QISRAudioWrite(sess_id, bp, 1, status, ref ep_status, ref rec_status);
                if (ret != 0) throw new Exception("QISRAudioWrite write last audio err,errCode=" + ((ErrorCode)ret).ToString("G"));
                Marshal.FreeHGlobal(bp);
                int loop_count = 0;
                ///最后一块数据发完之后，循环从服务器端获取结果
                ///考虑到网络环境不好的情况下，需要对循环次数作限定
                do
                {
                    IntPtr p = ASRDll.QISRGetResult(sess_id, ref rslt_status, waitTime, ref ret);
                    if (p != IntPtr.Zero)
                    {
                        string tmp = Ptr2Str(p);
                        DataArrived(this, new DataArrivedEventArgs(tmp));//激发识别数据到达事件
                        result += tmp;
                        System.Console.WriteLine("传完音频后返回结果！:" + tmp);
                    }
                    if (ret != 0) throw new Exception("QISRGetResult err,errCode=" + ((ErrorCode)ret).ToString("G"));
                    System.Threading.Thread.Sleep(200);
                } while (rslt_status != RecogStatus.ISR_REC_STATUS_SPEECH_COMPLETE && loop_count++ < 30);
                if (outFile != null)
                {
                    FileStream fout = new FileStream(outFile, FileMode.OpenOrCreate);
                    fout.Write(Encoding.Default.GetBytes(result), 0, Encoding.Default.GetByteCount(result));
                    fout.Close();
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
            finally
            {
                ret = ASRDll.QISRSessionEnd(sess_id, string.Empty);
                ret = ASRDll.QISRFini();
                ISREnd(this, new EventArgs());//通知识别结束
            }
        }

        #region 想实现编录边识别的功能，但还不行
        public void StartRecoding()
        {
            wis.StartRecording();
        }

        public void StopRecoding()
        {
            wis.StopRecording();
            AudioStatus audioStatus = AudioStatus.ISR_AUDIO_SAMPLE_LAST;
            EpStatus ep_status = EpStatus.ISR_EP_NULL;
            RecogStatus rec_status = RecogStatus.ISR_REC_NULL;
            RecogStatus rslt_status = RecogStatus.ISR_REC_NULL;

            int ret = ASRDll.QISRAudioWrite(sess_id, IntPtr.Zero, 0, audioStatus, ref ep_status, ref rec_status);
            if (ret != 0) throw new Exception("QISRAudioWrite write last audio err,errCode=" + ((ErrorCode)ret).ToString("G"));

            do
            {
                IntPtr p = ASRDll.QISRGetResult(sess_id, ref rslt_status, 0, ref ret);
                if (ret != 0) throw new Exception("QISRGetResult err,errCode=" + ((ErrorCode)ret).ToString("G"));
                if (p != IntPtr.Zero)
                {
                    string tmp = Ptr2Str(p);
                    DataArrived(this, new DataArrivedEventArgs(tmp));//激发识别数据到达事件
                    System.Console.WriteLine("传完音频后返回结果！-->" + tmp);
                }
                System.Threading.Thread.Sleep(200);
            } while (rslt_status != RecogStatus.ISR_REC_STATUS_SPEECH_COMPLETE);

            ret = ASRDll.QISRSessionEnd(sess_id, string.Empty);
            ret = ASRDll.QISRFini();
            ISREnd(this, new EventArgs());//通知识别结束

        }

        private void wis_DataAvailable(object sender, WaveInEventArgs e)
        {
            System.Console.WriteLine(System.DateTime.Now + ":" + System.DateTime.Now.Millisecond);

            int ret = 0;
            IntPtr bp = Marshal.AllocHGlobal(e.BytesRecorded);
            AudioStatus status = AudioStatus.ISR_AUDIO_SAMPLE_CONTINUE;
            EpStatus ep_status = EpStatus.ISR_EP_NULL;
            RecogStatus rec_status = RecogStatus.ISR_REC_NULL;
            RecogStatus rslt_status = RecogStatus.ISR_REC_NULL;
            ///ep_status        端点检测（End-point detected）器所处的状态
            ///rec_status       识别器所处的状态
            ///rslt_status      识别器所处的状态
            Marshal.Copy(e.Buffer, 0, bp, e.BytesRecorded);
            ///开始向服务器发送音频数据
            ret = ASRDll.QISRAudioWrite(sess_id, bp, (uint)e.BytesRecorded, status, ref ep_status, ref rec_status);
            if (ret != 0) throw new Exception("QISRAudioWrite err,errCode=" + ((ErrorCode)ret).ToString("G"));

            ///服务器返回部分结果
            if (rec_status == RecogStatus.ISR_REC_STATUS_SUCCESS)
            {
                IntPtr p = ASRDll.QISRGetResult(sess_id, ref rslt_status, 0, ref ret);
                if (p != IntPtr.Zero)
                {
                    string tmp = Ptr2Str(p);
                    DataArrived(this, new DataArrivedEventArgs(tmp));//激发识别数据到达事件
                    System.Console.WriteLine("服务器返回部分结果！-->" + tmp);
                }
            }
            System.Threading.Thread.Sleep(500);
            Marshal.FreeHGlobal(bp);
            //System.Console.WriteLine(System.DateTime.Now + ":" + System.DateTime.Now.Millisecond);
        }
        #endregion

        /// <summary>
        /// 指针转字符串
        /// </summary>
        /// <param name="p">指向非托管代码字符串的指针</param>
        /// <returns>返回指针指向的字符串</returns>
        private string Ptr2Str(IntPtr p)
        {
            List<byte> lb = new List<byte>();
            while (Marshal.ReadByte(p) != 0)
            {
                lb.Add(Marshal.ReadByte(p));
                p = p + 1;
            }
            byte[] bs = lb.ToArray();
            return Encoding.Default.GetString(lb.ToArray());
        }
    }
}
