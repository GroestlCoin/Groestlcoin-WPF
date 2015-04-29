#pragma once

#ifndef UCFG_BITCOIN_ASM
#	define UCFG_BITCOIN_ASM UCFG_USE_MASM
#endif

#ifndef UCFG_BITCOIN_ASM_X86
#	ifdef _M_IX86
#		define UCFG_BITCOIN_ASM_X86 UCFG_BITCOIN_ASM
#	else
#		define UCFG_BITCOIN_ASM_X86 0
#	endif
#endif

#ifndef UCFG_BITCOIN_TRACE
#	define UCFG_BITCOIN_TRACE 0
#endif

#ifndef UCFG_BITCOIN_USE_CAL
#	define UCFG_BITCOIN_USE_CAL 0
#endif

#ifndef UCFG_BITCOIN_USE_CUDA
#	define UCFG_BITCOIN_USE_CUDA 0
#endif

#ifndef UCFG_BITCOIN_THERMAL_CONTROL
#	define UCFG_BITCOIN_THERMAL_CONTROL UCFG_WIN32
#endif

#ifndef UCFG_BITCOIN_USERAGENT_INFO
#	define UCFG_BITCOIN_USERAGENT_INFO 1
#endif

#ifndef UCFG_BITCOIN_WAY
#	define UCFG_BITCOIN_WAY 4
#endif

#ifndef UCFG_BITCOIN_NPAR
#	define UCFG_BITCOIN_NPAR 128
#endif


#if UCFG_LIB_DECLS
#	ifdef _MINER
#		define MINER_CLASS       AFX_CLASS_EXPORT
#	else
#		define MINER_CLASS       AFX_CLASS_IMPORT
#	endif
#else
#	define MINER_CLASS
#endif

#ifndef UCFG_COIN_PRIME
#	define UCFG_COIN_PRIME 1
#endif

#ifndef UCFG_BITCOIN_SOLIDCOIN
#	define UCFG_BITCOIN_SOLIDCOIN 1
#endif

#ifndef UCFG_BITCOIN_USE_RESOURCE
#	define UCFG_BITCOIN_USE_RESOURCE 0
#endif

//#define UCFG_BITCOIN_USE_CRASHDUMP 0

#ifndef UCFG_BITCOIN_USE_CRASHDUMP
#	if UCFG_WIN32 && UCFG_DEBUG //&& !defined(_AFXDLL) && !defined(_M_X64)
#		define UCFG_BITCOIN_USE_CRASHDUMP 1
#	else
#		define UCFG_BITCOIN_USE_CRASHDUMP 0
#	endif
#endif

#ifndef UCFG_COIN_MINER_SERVICE
#	define UCFG_COIN_MINER_SERVICE 1
#endif

