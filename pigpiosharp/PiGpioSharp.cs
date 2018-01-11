﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

namespace PiGpio
{
    public class CommandFailedException : Exception
    {
        static Dictionary<ErrorCode, string> errorMessages = new Dictionary<ErrorCode, string>
            {
               { ErrorCode.PI_INIT_FAILED      , "pigpio initialisation failed"},
               { ErrorCode.PI_BAD_USER_GPIO    , "GPIO not 0-31"},
               { ErrorCode.PI_BAD_GPIO         , "GPIO not 0-53"},
               { ErrorCode.PI_BAD_MODE         , "mode not 0-7"},
               { ErrorCode.PI_BAD_LEVEL        , "level not 0-1"},
               { ErrorCode.PI_BAD_PUD          , "pud not 0-2"},
               { ErrorCode.PI_BAD_PULSEWIDTH   , "pulsewidth not 0 or 500-2500"},
               { ErrorCode.PI_BAD_DUTYCYCLE    , "dutycycle not 0-range (default 255)"},
               { ErrorCode.PI_BAD_TIMER        , "timer not 0-9"},
               { ErrorCode.PI_BAD_MS           , "ms not 10-60000"},
               { ErrorCode.PI_BAD_TIMETYPE     , "timetype not 0-1"},
               { ErrorCode.PI_BAD_SECONDS      , "seconds < 0"},
               { ErrorCode.PI_BAD_MICROS       , "micros not 0-999999"},
               { ErrorCode.PI_TIMER_FAILED     , "gpioSetTimerFunc failed"},
               { ErrorCode.PI_BAD_WDOG_TIMEOUT , "timeout not 0-60000"},
               { ErrorCode.PI_NO_ALERT_FUNC    , "DEPRECATED"},
               { ErrorCode.PI_BAD_CLK_PERIPH   , "clock peripheral not 0-1"},
               { ErrorCode.PI_BAD_CLK_SOURCE   , "DEPRECATED"},
               { ErrorCode.PI_BAD_CLK_MICROS   , "clock micros not 1, 2, 4, 5, 8, or 10"},
               { ErrorCode.PI_BAD_BUF_MILLIS   , "buf millis not 100-10000"},
               { ErrorCode.PI_BAD_DUTYRANGE    , "dutycycle range not 25-40000"},
               { ErrorCode.PI_BAD_SIGNUM       , "signum not 0-63"},
               { ErrorCode.PI_BAD_PATHNAME     , "can't open pathname"},
               { ErrorCode.PI_NO_HANDLE        , "no handle available"},
               { ErrorCode.PI_BAD_HANDLE       , "unknown handle"},
               { ErrorCode.PI_BAD_IF_FLAGS     , "ifFlags > 3"},
               { ErrorCode.PI_BAD_CHANNEL      , "DMA channel not 0-14"},
               { ErrorCode.PI_BAD_SOCKET_PORT  , "socket port not 1024-30000"},
               { ErrorCode.PI_BAD_FIFO_COMMAND , "unknown fifo command"},
               { ErrorCode.PI_BAD_SECO_CHANNEL , "DMA secondary channel not 0-14"},
               { ErrorCode.PI_NOT_INITIALISED  , "function called before gpioInitialise"},
               { ErrorCode.PI_INITIALISED      , "function called after gpioInitialise"},
               { ErrorCode.PI_BAD_WAVE_MODE    , "waveform mode not 0-1"},
               { ErrorCode.PI_BAD_CFG_INTERNAL , "bad parameter in gpioCfgInternals call"},
               { ErrorCode.PI_BAD_WAVE_BAUD    , "baud rate not 50-250K(RX)/50-1M(TX)"},
               { ErrorCode.PI_TOO_MANY_PULSES  , "waveform has too many pulses"},
               { ErrorCode.PI_TOO_MANY_CHARS   , "waveform has too many chars"},
               { ErrorCode.PI_NOT_SERIAL_GPIO  , "no bit bang serial read in progress on GPIO"},
               { ErrorCode.PI_BAD_SERIAL_STRUC , "bad (null) serial structure parameter"},
               { ErrorCode.PI_BAD_SERIAL_BUF   , "bad (null) serial buf parameter"},
               { ErrorCode.PI_NOT_PERMITTED    , "no permission to update GPIO"},
               { ErrorCode.PI_SOME_PERMITTED   , "no permission to update one or more GPIO"},
               { ErrorCode.PI_BAD_WVSC_COMMND  , "bad WVSC subcommand"},
               { ErrorCode.PI_BAD_WVSM_COMMND  , "bad WVSM subcommand"},
               { ErrorCode.PI_BAD_WVSP_COMMND  , "bad WVSP subcommand"},
               { ErrorCode.PI_BAD_PULSELEN     , "trigger pulse length not 1-100"},
               { ErrorCode.PI_BAD_SCRIPT       , "invalid script"},
               { ErrorCode.PI_BAD_SCRIPT_ID    , "unknown script id"},
               { ErrorCode.PI_BAD_SER_OFFSET   , "add serial data offset > 30 minute"},
               { ErrorCode.PI_GPIO_IN_USE      , "GPIO already in use"},
               { ErrorCode.PI_BAD_SERIAL_COUNT , "must read at least a byte at a time"},
               { ErrorCode.PI_BAD_PARAM_NUM    , "script parameter id not 0-9"},
               { ErrorCode.PI_DUP_TAG          , "script has duplicate tag"},
               { ErrorCode.PI_TOO_MANY_TAGS    , "script has too many tags"},
               { ErrorCode.PI_BAD_SCRIPT_CMD   , "illegal script command"},
               { ErrorCode.PI_BAD_VAR_NUM      , "script variable id not 0-149"},
               { ErrorCode.PI_NO_SCRIPT_ROOM   , "no more room for scripts"},
               { ErrorCode.PI_NO_MEMORY        , "can't allocate temporary memory"},
               { ErrorCode.PI_SOCK_READ_FAILED , "socket read failed"},
               { ErrorCode.PI_SOCK_WRIT_FAILED , "socket write failed"},
               { ErrorCode.PI_TOO_MANY_PARAM   , "too many script parameters (> 10)"},
               { ErrorCode.PI_SCRIPT_NOT_READY , "script initialising"},
               { ErrorCode.PI_BAD_TAG          , "script has unresolved tag"},
               { ErrorCode.PI_BAD_MICS_DELAY   , "bad MICS delay (too large)"},
               { ErrorCode.PI_BAD_MILS_DELAY   , "bad MILS delay (too large)"},
               { ErrorCode.PI_BAD_WAVE_ID      , "non existent wave id"},
               { ErrorCode.PI_TOO_MANY_CBS     , "No more CBs for waveform"},
               { ErrorCode.PI_TOO_MANY_OOL     , "No more OOL for waveform"},
               { ErrorCode.PI_EMPTY_WAVEFORM   , "attempt to create an empty waveform"},
               { ErrorCode.PI_NO_WAVEFORM_ID   , "no more waveform ids"},
               { ErrorCode.PI_I2C_OPEN_FAILED  , "can't open I2C device"},
               { ErrorCode.PI_SER_OPEN_FAILED  , "can't open serial device"},
               { ErrorCode.PI_SPI_OPEN_FAILED  , "can't open SPI device"},
               { ErrorCode.PI_BAD_I2C_BUS      , "bad I2C bus"},
               { ErrorCode.PI_BAD_I2C_ADDR     , "bad I2C address"},
               { ErrorCode.PI_BAD_SPI_CHANNEL  , "bad SPI channel"},
               { ErrorCode.PI_BAD_FLAGS        , "bad i2c/spi/ser open flags"},
               { ErrorCode.PI_BAD_SPI_SPEED    , "bad SPI speed"},
               { ErrorCode.PI_BAD_SER_DEVICE   , "bad serial device name"},
               { ErrorCode.PI_BAD_SER_SPEED    , "bad serial baud rate"},
               { ErrorCode.PI_BAD_PARAM        , "bad i2c/spi/ser parameter"},
               { ErrorCode.PI_I2C_WRITE_FAILED , "I2C write failed"},
               { ErrorCode.PI_I2C_READ_FAILED  , "I2C read failed"},
               { ErrorCode.PI_BAD_SPI_COUNT    , "bad SPI count"},
               { ErrorCode.PI_SER_WRITE_FAILED , "ser write failed"},
               { ErrorCode.PI_SER_READ_FAILED  , "ser read failed"},
               { ErrorCode.PI_SER_READ_NO_DATA , "ser read no data available"},
               { ErrorCode.PI_UNKNOWN_COMMAND  , "unknown command"},
               { ErrorCode.PI_SPI_XFER_FAILED  , "spi xfer/read/write failed"},
               { ErrorCode.PI_BAD_POINTER      , "bad (NULL) pointer"},
               { ErrorCode.PI_NO_AUX_SPI       , "no auxiliary SPI on Pi A or B"},
               { ErrorCode.PI_NOT_PWM_GPIO     , "GPIO is not in use for PWM"},
               { ErrorCode.PI_NOT_SERVO_GPIO   , "GPIO is not in use for servo pulses"},
               { ErrorCode.PI_NOT_HCLK_GPIO    , "GPIO has no hardware clock"},
               { ErrorCode.PI_NOT_HPWM_GPIO    , "GPIO has no hardware PWM"},
               { ErrorCode.PI_BAD_HPWM_FREQ    , "hardware PWM frequency not 1-125M"},
               { ErrorCode.PI_BAD_HPWM_DUTY    , "hardware PWM dutycycle not 0-1M"},
               { ErrorCode.PI_BAD_HCLK_FREQ    , "hardware clock frequency not 4689-250M"},
               { ErrorCode.PI_BAD_HCLK_PASS    , "need password to use hardware clock 1"},
               { ErrorCode.PI_HPWM_ILLEGAL     , "illegal, PWM in use for main clock"},
               { ErrorCode.PI_BAD_DATABITS     , "serial data bits not 1-32"},
               { ErrorCode.PI_BAD_STOPBITS     , "serial (half) stop bits not 2-8"},
               { ErrorCode.PI_MSG_TOOBIG       , "socket/pipe message too big"},
               { ErrorCode.PI_BAD_MALLOC_MODE  , "bad memory allocation mode"},
               { ErrorCode.PI_TOO_MANY_SEGS    , "too many I2C transaction segments"},
               { ErrorCode.PI_BAD_I2C_SEG      , "an I2C transaction segment failed"},
               { ErrorCode.PI_BAD_SMBUS_CMD    , "SMBus command not supported by driver"},
               { ErrorCode.PI_NOT_I2C_GPIO     , "no bit bang I2C in progress on GPIO"},
               { ErrorCode.PI_BAD_I2C_WLEN     , "bad I2C write length"},
               { ErrorCode.PI_BAD_I2C_RLEN     , "bad I2C read length"},
               { ErrorCode.PI_BAD_I2C_CMD      , "bad I2C command"},
               { ErrorCode.PI_BAD_I2C_BAUD     , "bad I2C baud rate, not 50-500k"},
               { ErrorCode.PI_CHAIN_LOOP_CNT   , "bad chain loop count"},
               { ErrorCode.PI_BAD_CHAIN_LOOP   , "empty chain loop"},
               { ErrorCode.PI_CHAIN_COUNTER    , "too many chain counters"},
               { ErrorCode.PI_BAD_CHAIN_CMD    , "bad chain command"},
               { ErrorCode.PI_BAD_CHAIN_DELAY  , "bad chain delay micros"},
               { ErrorCode.PI_CHAIN_NESTING    , "chain counters nested too deeply"},
               { ErrorCode.PI_CHAIN_TOO_BIG    , "chain is too long"},
               { ErrorCode.PI_DEPRECATED       , "deprecated function removed"},
               { ErrorCode.PI_BAD_SER_INVERT   , "bit bang serial invert not 0 or 1"},
               { ErrorCode.PI_BAD_EDGE         , "bad ISR edge, not 1, 1, or 2"},
               { ErrorCode.PI_BAD_ISR_INIT     , "bad ISR initialisation"},
               { ErrorCode.PI_BAD_FOREVER      , "loop forever must be last chain command"},
               { ErrorCode.PI_BAD_FILTER       , "bad filter parameter"},
               { ErrorCode.PI_BAD_PAD          , "bad pad number"},
               { ErrorCode.PI_BAD_STRENGTH     , "bad pad drive strength"},
               { ErrorCode.PI_FIL_OPEN_FAILED  , "file open failed"},
               { ErrorCode.PI_BAD_FILE_MODE    , "bad file mode"},
               { ErrorCode.PI_BAD_FILE_FLAG    , "bad file flag"},
               { ErrorCode.PI_BAD_FILE_READ    , "bad file read"},
               { ErrorCode.PI_BAD_FILE_WRITE   , "bad file write"},
               { ErrorCode.PI_FILE_NOT_ROPEN   , "file not open for read"},
               { ErrorCode.PI_FILE_NOT_WOPEN   , "file not open for write"},
               { ErrorCode.PI_BAD_FILE_SEEK    , "bad file seek"},
               { ErrorCode.PI_NO_FILE_MATCH    , "no files match pattern"},
               { ErrorCode.PI_NO_FILE_ACCESS   , "no permission to access file"},
               { ErrorCode.PI_FILE_IS_A_DIR    , "file is a directory"},
               { ErrorCode.PI_BAD_SHELL_STATUS , "bad shell return status"},
               { ErrorCode.PI_BAD_SCRIPT_NAME  , "bad script name"},
               { ErrorCode.PI_BAD_SPI_BAUD     , "bad SPI baud rate, not 50-500k"},
               { ErrorCode.PI_NOT_SPI_GPIO     , "no bit bang SPI in progress on GPIO"},
               { ErrorCode.PI_BAD_EVENT_ID     , "bad event id"}
            };
        ErrorCode m_errorcode;

        public CommandFailedException(ErrorCode e)
        {
            m_errorcode = e;
        }

        public override string Message
        {
            get
            {
                return errorMessages[m_errorcode];
            }
        }

        public ErrorCode Error
        {
            get
            {
                return m_errorcode;
            }
        }
    }

    public enum CommandCode
    {
        PI_CMD_MODES = 0,
        PI_CMD_MODEG = 1,
        PI_CMD_PUD = 2,
        PI_CMD_READ = 3,
        PI_CMD_WRITE = 4,
        PI_CMD_PWM = 5,
        PI_CMD_PRS = 6,
        PI_CMD_PFS = 7,
        PI_CMD_SERVO = 8,
        PI_CMD_WDOG = 9,
        PI_CMD_BR1 = 10,
        PI_CMD_BR2 = 11,
        PI_CMD_BC1 = 12,
        PI_CMD_BC2 = 13,
        PI_CMD_BS1 = 14,
        PI_CMD_BS2 = 15,
        PI_CMD_TICK = 16,
        PI_CMD_HWVER = 17,

        PI_CMD_NO = 18,
        PI_CMD_NB = 19,
        PI_CMD_NP = 20,
        PI_CMD_NC = 21,

        PI_CMD_PRG = 22,
        PI_CMD_PFG = 23,
        PI_CMD_PRRG = 24,
        PI_CMD_HELP = 25,
        PI_CMDPIGPV = 26,

        PI_CMD_WVCLR = 27,
        PI_CMD_WVAG = 28,
        PI_CMD_WVAS = 29,
        PI_CMD_WVGO = 30,
        PI_CMD_WVGOR = 31,
        PI_CMD_WVBSY = 32,
        PI_CMD_WVHLT = 33,
        PI_CMD_WVSM = 34,
        PI_CMD_WVSP = 35,
        PI_CMD_WVSC = 36,

        PI_CMD_TRIG = 37,

        PI_CMD_PROC = 38,
        PI_CMD_PROCD = 39,
        PI_CMD_PROCR = 40,
        PI_CMD_PROCS = 41,

        PI_CMD_SLRO = 42,
        PI_CMD_SLR = 43,
        PI_CMD_SLRC = 44,

        PI_CMD_PROCP = 45,
        PI_CMD_MICRO = 46,
        PI_CMD_MILLI = 47,
        PI_CMD_PARSE = 48,

        PI_CMD_WVCRE = 49,
        PI_CMD_WVDEL = 50,
        PI_CMD_WVTX = 51,
        PI_CMD_WVTXR = 52,
        PI_CMD_WVNEW = 53,

        PI_CMD_I2CO = 54,
        PI_CMD_I2CC = 55,
        PI_CMD_I2CRD = 56,
        PI_CMD_I2CWD = 57,
        PI_CMD_I2CWQ = 58,
        PI_CMD_I2CRS = 59,
        PI_CMD_I2CWS = 60,
        PI_CMD_I2CRB = 61,
        PI_CMD_I2CWB = 62,
        PI_CMD_I2CRW = 63,
        PI_CMD_I2CWW = 64,
        PI_CMD_I2CRK = 65,
        PI_CMD_I2CWK = 66,
        PI_CMD_I2CRI = 67,
        PI_CMD_I2CWI = 68,
        PI_CMD_I2CPC = 69,
        PI_CMD_I2CPK = 70,

        PI_CMD_SPIO = 71,
        PI_CMD_SPIC = 72,
        PI_CMD_SPIR = 73,
        PI_CMD_SPIW = 74,
        PI_CMD_SPIX = 75,

        PI_CMD_SERO = 76,
        PI_CMD_SERC = 77,
        PI_CMD_SERRB = 78,
        PI_CMD_SERWB = 79,
        PI_CMD_SERR = 80,
        PI_CMD_SERW = 81,
        PI_CMD_SERDA = 82,

        PI_CMD_GDC = 83,
        PI_CMD_GPW = 84,

        PI_CMD_HC = 85,
        PI_CMD_HP = 86,

        PI_CMD_CF1 = 87,
        PI_CMD_CF2 = 88,

        PI_CMD_NOIB = 99,

        PI_CMD_BI2CC = 89,
        PI_CMD_BI2CO = 90,
        PI_CMD_BI2CZ = 91,

        PI_CMD_I2CZ = 92,

        PI_CMD_WVCHA = 93,

        PI_CMD_SLRI = 94,

        PI_CMD_CGI = 95,
        PI_CMD_CSI = 96,

        PI_CMD_FG = 97,
        PI_CMD_FN = 98,

        PI_CMD_WVTXM = 100,
        PI_CMD_WVTAT = 101,

        PI_CMD_PADS = 102,
        PI_CMD_PADG = 103,

        PI_CMD_FO = 104,
        PI_CMD_FC = 105,
        PI_CMD_FR = 106,
        PI_CMD_FW = 107,
        PI_CMD_FS = 108,
        PI_CMD_FL = 109,
        PI_CMD_SHELL = 110,

        PI_CMD_BSPIC = 111,
        PI_CMD_BSPIO = 112,
        PI_CMD_BSPIX = 113,

        PI_CMD_BSCX = 114,

        PI_CMD_EVM = 115,
        PI_CMD_EVT = 116
    }

    public enum ErrorCode
    {
        PI_INIT_FAILED = -1, // gpioInitialise failed
        PI_BAD_USER_GPIO = -2, // GPIO not 0-31
        PI_BAD_GPIO = -3, // GPIO not 0-53
        PI_BAD_MODE = -4, // mode not 0-7
        PI_BAD_LEVEL = -5, // level not 0-1
        PI_BAD_PUD = -6, // pud not 0-2
        PI_BAD_PULSEWIDTH = -7, // pulsewidth not 0 or 500-2500
        PI_BAD_DUTYCYCLE = -8, // dutycycle outside set range
        PI_BAD_TIMER = -9, // timer not 0-9
        PI_BAD_MS = -10, // ms not 10-60000
        PI_BAD_TIMETYPE = -11, // timetype not 0-1
        PI_BAD_SECONDS = -12, // seconds < 0
        PI_BAD_MICROS = -13, // micros not 0-999999
        PI_TIMER_FAILED = -14, // gpioSetTimerFunc failed
        PI_BAD_WDOG_TIMEOUT = -15, // timeout not 0-60000
        PI_NO_ALERT_FUNC = -16, // DEPRECATED
        PI_BAD_CLK_PERIPH = -17, // clock peripheral not 0-1
        PI_BAD_CLK_SOURCE = -18, // DEPRECATED
        PI_BAD_CLK_MICROS = -19, // clock micros not 1, 2, 4, 5, 8, or 10
        PI_BAD_BUF_MILLIS = -20, // buf millis not 100-10000
        PI_BAD_DUTYRANGE = -21, // dutycycle range not 25-40000
        PI_BAD_DUTY_RANGE = -21, // DEPRECATED (use PI_BAD_DUTYRANGE)
        PI_BAD_SIGNUM = -22, // signum not 0-63
        PI_BAD_PATHNAME = -23, // can't open pathname
        PI_NO_HANDLE = -24, // no handle available
        PI_BAD_HANDLE = -25, // unknown handle
        PI_BAD_IF_FLAGS = -26, // ifFlags > 3
        PI_BAD_CHANNEL = -27, // DMA channel not 0-14
        PI_BAD_PRIM_CHANNEL = -27, // DMA primary channel not 0-14
        PI_BAD_SOCKET_PORT = -28, // socket port not 1024-32000
        PI_BAD_FIFO_COMMAND = -29, // unrecognized fifo command
        PI_BAD_SECO_CHANNEL = -30, // DMA secondary channel not 0-6
        PI_NOT_INITIALISED = -31, // function called before gpioInitialise
        PI_INITIALISED = -32, // function called after gpioInitialise
        PI_BAD_WAVE_MODE = -33, // waveform mode not 0-3
        PI_BAD_CFG_INTERNAL = -34, // bad parameter in gpioCfgInternals call
        PI_BAD_WAVE_BAUD = -35, // baud rate not 50-250K(RX)/50-1M(TX)
        PI_TOO_MANY_PULSES = -36, // waveform has too many pulses
        PI_TOO_MANY_CHARS = -37, // waveform has too many chars
        PI_NOT_SERIAL_GPIO = -38, // no bit bang serial read on GPIO
        PI_BAD_SERIAL_STRUC = -39, // bad (null) serial structure parameter
        PI_BAD_SERIAL_BUF = -40, // bad (null) serial buf parameter
        PI_NOT_PERMITTED = -41, // GPIO operation not permitted
        PI_SOME_PERMITTED = -42, // one or more GPIO not permitted
        PI_BAD_WVSC_COMMND = -43, // bad WVSC subcommand
        PI_BAD_WVSM_COMMND = -44, // bad WVSM subcommand
        PI_BAD_WVSP_COMMND = -45, // bad WVSP subcommand
        PI_BAD_PULSELEN = -46, // trigger pulse length not 1-100
        PI_BAD_SCRIPT = -47, // invalid script
        PI_BAD_SCRIPT_ID = -48, // unknown script id
        PI_BAD_SER_OFFSET = -49, // add serial data offset > 30 minutes
        PI_GPIO_IN_USE = -50, // GPIO already in use
        PI_BAD_SERIAL_COUNT = -51, // must read at least a byte at a time
        PI_BAD_PARAM_NUM = -52, // script parameter id not 0-9
        PI_DUP_TAG = -53, // script has duplicate tag
        PI_TOO_MANY_TAGS = -54, // script has too many tags
        PI_BAD_SCRIPT_CMD = -55, // illegal script command
        PI_BAD_VAR_NUM = -56, // script variable id not 0-149
        PI_NO_SCRIPT_ROOM = -57, // no more room for scripts
        PI_NO_MEMORY = -58, // can't allocate temporary memory
        PI_SOCK_READ_FAILED = -59, // socket read failed
        PI_SOCK_WRIT_FAILED = -60, // socket write failed
        PI_TOO_MANY_PARAM = -61, // too many script parameters (> 10)
        PI_NOT_HALTED = -62, // DEPRECATED
        PI_SCRIPT_NOT_READY = -62, // script initialising
        PI_BAD_TAG = -63, // script has unresolved tag
        PI_BAD_MICS_DELAY = -64, // bad MICS delay (too large)
        PI_BAD_MILS_DELAY = -65, // bad MILS delay (too large)
        PI_BAD_WAVE_ID = -66, // non existent wave id
        PI_TOO_MANY_CBS = -67, // No more CBs for waveform
        PI_TOO_MANY_OOL = -68, // No more OOL for waveform
        PI_EMPTY_WAVEFORM = -69, // attempt to create an empty waveform
        PI_NO_WAVEFORM_ID = -70, // no more waveforms
        PI_I2C_OPEN_FAILED = -71, // can't open I2C device
        PI_SER_OPEN_FAILED = -72, // can't open serial device
        PI_SPI_OPEN_FAILED = -73, // can't open SPI device
        PI_BAD_I2C_BUS = -74, // bad I2C bus
        PI_BAD_I2C_ADDR = -75, // bad I2C address
        PI_BAD_SPI_CHANNEL = -76, // bad SPI channel
        PI_BAD_FLAGS = -77, // bad i2c/spi/ser open flags
        PI_BAD_SPI_SPEED = -78, // bad SPI speed
        PI_BAD_SER_DEVICE = -79, // bad serial device name
        PI_BAD_SER_SPEED = -80, // bad serial baud rate
        PI_BAD_PARAM = -81, // bad i2c/spi/ser parameter
        PI_I2C_WRITE_FAILED = -82, // i2c write failed
        PI_I2C_READ_FAILED = -83, // i2c read failed
        PI_BAD_SPI_COUNT = -84, // bad SPI count
        PI_SER_WRITE_FAILED = -85, // ser write failed
        PI_SER_READ_FAILED = -86, // ser read failed
        PI_SER_READ_NO_DATA = -87, // ser read no data available
        PI_UNKNOWN_COMMAND = -88, // unknown command
        PI_SPI_XFER_FAILED = -89, // spi xfer/read/write failed
        PI_BAD_POINTER = -90, // bad (NULL) pointer
        PI_NO_AUX_SPI = -91, // no auxiliary SPI on Pi A or B
        PI_NOT_PWM_GPIO = -92, // GPIO is not in use for PWM
        PI_NOT_SERVO_GPIO = -93, // GPIO is not in use for servo pulses
        PI_NOT_HCLK_GPIO = -94, // GPIO has no hardware clock
        PI_NOT_HPWM_GPIO = -95, // GPIO has no hardware PWM
        PI_BAD_HPWM_FREQ = -96, // hardware PWM frequency not 1-125M
        PI_BAD_HPWM_DUTY = -97, // hardware PWM dutycycle not 0-1M
        PI_BAD_HCLK_FREQ = -98, // hardware clock frequency not 4689-250M
        PI_BAD_HCLK_PASS = -99, // need password to use hardware clock 1
        PI_HPWM_ILLEGAL = -100, // illegal, PWM in use for main clock
        PI_BAD_DATABITS = -101, // serial data bits not 1-32
        PI_BAD_STOPBITS = -102, // serial (half) stop bits not 2-8
        PI_MSG_TOOBIG = -103, // socket/pipe message too big
        PI_BAD_MALLOC_MODE = -104, // bad memory allocation mode
        PI_TOO_MANY_SEGS = -105, // too many I2C transaction segments
        PI_BAD_I2C_SEG = -106, // an I2C transaction segment failed
        PI_BAD_SMBUS_CMD = -107, // SMBus command not supported by driver
        PI_NOT_I2C_GPIO = -108, // no bit bang I2C in progress on GPIO
        PI_BAD_I2C_WLEN = -109, // bad I2C write length
        PI_BAD_I2C_RLEN = -110, // bad I2C read length
        PI_BAD_I2C_CMD = -111, // bad I2C command
        PI_BAD_I2C_BAUD = -112, // bad I2C baud rate, not 50-500k
        PI_CHAIN_LOOP_CNT = -113, // bad chain loop count
        PI_BAD_CHAIN_LOOP = -114, // empty chain loop
        PI_CHAIN_COUNTER = -115, // too many chain counters
        PI_BAD_CHAIN_CMD = -116, // bad chain command
        PI_BAD_CHAIN_DELAY = -117, // bad chain delay micros
        PI_CHAIN_NESTING = -118, // chain counters nested too deeply
        PI_CHAIN_TOO_BIG = -119, // chain is too long
        PI_DEPRECATED = -120, // deprecated function removed
        PI_BAD_SER_INVERT = -121, // bit bang serial invert not 0 or 1
        PI_BAD_EDGE = -122, // bad ISR edge value, not 0-2
        PI_BAD_ISR_INIT = -123, // bad ISR initialisation
        PI_BAD_FOREVER = -124, // loop forever must be last command
        PI_BAD_FILTER = -125, // bad filter parameter
        PI_BAD_PAD = -126, // bad pad number
        PI_BAD_STRENGTH = -127, // bad pad drive strength
        PI_FIL_OPEN_FAILED = -128, // file open failed
        PI_BAD_FILE_MODE = -129, // bad file mode
        PI_BAD_FILE_FLAG = -130, // bad file flag
        PI_BAD_FILE_READ = -131, // bad file read
        PI_BAD_FILE_WRITE = -132, // bad file write
        PI_FILE_NOT_ROPEN = -133, // file not open for read
        PI_FILE_NOT_WOPEN = -134, // file not open for write
        PI_BAD_FILE_SEEK = -135, // bad file seek
        PI_NO_FILE_MATCH = -136, // no files match pattern
        PI_NO_FILE_ACCESS = -137, // no permission to access file
        PI_FILE_IS_A_DIR = -138, // file is a directory
        PI_BAD_SHELL_STATUS = -139, // bad shell return status
        PI_BAD_SCRIPT_NAME = -140, // bad script name
        PI_BAD_SPI_BAUD = -141, // bad SPI baud rate, not 50-500k
        PI_NOT_SPI_GPIO = -142, // no bit bang SPI in progress on GPIO
        PI_BAD_EVENT_ID = -143  // bad event id
    }

    public class PiGpioSharp : IDisposable
    {
        string m_host;
        int m_port;
        Socket m_socket;
        Mutex m_lock;
        const int SOCKET_CMD_RESP_LENGTH = 16;

        public PiGpioSharp(string host = "localhost", int port = 8888)
        {
            m_host = host;
            m_port = port;
            m_socket = Connect();
            m_lock = new Mutex();
        }

        public PiGpioSharp()
        {
            m_host = Environment.GetEnvironmentVariable("PIGPIO_ADDR");
            var port = Environment.GetEnvironmentVariable("PIGPIO_PORT");
            try
            {
                m_port = int.Parse(port);
            }
            catch (Exception)
            {
                m_port = 8888;
            }
            if (m_host == null)
            {
                m_host = "localhost";
            }
            m_socket = Connect();
            m_lock = new Mutex();
        }

        public int Port
        {
            get { return m_port; }
        }

        public string Host
        {
            get { return m_host; }
        }

        public Socket Connect()
        {

            IPAddress ipAddress = Dns.GetHostAddresses(m_host)[0];
            var remote = new IPEndPoint(ipAddress, m_port);

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(remote);
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);
            return socket;
        }

        public static byte[] GetBytesFromInt32EndianessCorrected(int val)
        {
            var byteArray = BitConverter.GetBytes(val);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(byteArray);
            }
            return byteArray;
        }

		public static int GetInt32FromByteArrayEndianessCorrected(byte[] byteArray, int startIndex)
		{
            var tmp = new byte[4];
            Array.Copy(byteArray, startIndex, tmp, 0, tmp.Length);
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
            return BitConverter.ToInt32(tmp, 0);
		}

		public static int GetInt16FromByteArrayEndianessCorrected(byte[] byteArray, int startIndex)
		{
			var tmp = new byte[2];
			Array.Copy(byteArray, startIndex, tmp, 0, tmp.Length);
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse(tmp);
			}
			return BitConverter.ToInt16(tmp, 0);
		}

        public int ExecuteCommand(CommandCode command, int p1, int p2, int p3 = 0, byte[] ext = null, bool releaseLock = true)
        {
            int ret;
            m_lock.WaitOne();
            try
            {
                ret = ExecuteCommand(m_socket, command, p1, p2, p3, ext);
            }
            finally
            {
                if (releaseLock)
                {
                    m_lock.ReleaseMutex();
                }
            }

            return ret;
        }

        public void ReleaseLock()
        {
            m_lock.ReleaseMutex();
        }

        public int ExecuteCommand(Socket s, CommandCode command, int p1, int p2, int p3 = 0, byte[] ext = null)
        {
            var data = new List<byte>();
            byte[] resp = new byte[SOCKET_CMD_RESP_LENGTH];

            data.AddRange(GetBytesFromInt32EndianessCorrected((int)command));
            data.AddRange(GetBytesFromInt32EndianessCorrected(p1));
            data.AddRange(GetBytesFromInt32EndianessCorrected(p2));
            data.AddRange(GetBytesFromInt32EndianessCorrected(p3));
            if (ext != null)
            {
                data.AddRange(ext);
            }
            s.Send(data.ToArray());
            resp = GetMessage(s, SOCKET_CMD_RESP_LENGTH);
            var res = GetInt32FromByteArrayEndianessCorrected(resp, 12);
            if (res < 0)
            {
                throw new CommandFailedException((ErrorCode)res);
            }
            return res;
        }

        public byte[] GetMessage(Socket s, int count)
        {
			byte[] resp = new byte[count];
			int totalBytes = 0;
			int bytesReceived;

			do
			{
				bytesReceived = s.Receive(resp, totalBytes, count - totalBytes, SocketFlags.None);
				totalBytes += bytesReceived;
			} while (totalBytes < count);
			return resp;
        }

        public byte[] GetMessage(int count)
        {
            return GetMessage(m_socket, count);
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
          if (!disposedValue)
          {
            if (disposing)
            {
                m_lock.Close();
            }

            disposedValue = true;
          }
        }

        public void Dispose()
        {
          Dispose(true);
        }
        #endregion
  }
}
