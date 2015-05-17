#define _USRDLL

#define UCFG_USE_RTTI 1

#define UCFG_TRC 1
#define UCFG_USE_LIBXML 0

#define UCFG_USE_TOR UCFG_WIN32_FULL

#define UCFG_EXPORT_COIN

#define VER_FILEDESCRIPTION_STR "Coin Engine"
#define VER_INTERNALNAME_STR "GroestlCoin-WPF"
#define VER_ORIGINALFILENAME_STR "coineng.dll"

#ifdef _DEBUG//!!!D
#	define UCFG_COIN_PKSCRIPT_FUTURES 0
#	define UCFG_COIN_MERKLE_FUTURES 0
#endif


