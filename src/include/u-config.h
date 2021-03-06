#pragma once

#include <manufacturer.h>

#define UCFG_BITCOIN_USE_CAL 0

#define UCFG_STDSTL 1
#define UCFG_LIB_DECLS 0

#define TICKSPERSECOND 10

#define _WIN32_WINNT 0x0600

#define UCFG_USE_BOOST 0
#define UCFG_STLSOFT 0
#define UCFG_USE_RTTI 1
#define UCFG_USE_OPENSSL 0

#define UCFG_EH_SUPPORT_IGNORE 0

#define UCFG_USELISP 0
#define UCFG_GUI 0
#define UCFG_UPGRADE 0
#define ENSURE_COPY_PROT
#define UCFG_WIN_MSG 0
#define UCFG_WND 0
#define UCFG_USE_SIMPLE_MAPI 0

#ifndef WINVER
	#define WINVER 0x0500
#endif

#ifndef _WIN32_WINNT
	#define _WIN32_WINNT 0x500
#endif

#ifndef WIN32
#	define WIN32
#endif


#ifndef NTDDI_XPSP1
	#define NTDDI_XPSP1                      0x05010100
#endif

#define TRACE_ENTER(x)
#define TRACE_EXIT(x)
#define TRACE_PRINT(s)
#define TRACE_PRINT1(s, a)
#define TRACE_PRINT2(s, a, b)
#define TRACE_PRINT6(s, a, b, c, d, e, f)

#define _SECURE_COMPILER_COM 0

#define _CRT_INSECURE_NO_DEPRECATE  //VS8
#define _HAS_STRICT_CONFORMANCE 1

#define _HAS_ITERATOR_DEBUGGING 0


#define _WINSOCKAPI_

#define _CRT_WCTYPE_NOINLINE


#define UCFG_SECURE_SCL 2


#define _CRT_NON_CONFORMING_SWPRINTFS


#define _ATL_DLL_IMPL

#ifndef UCFG_INTRINSIC_MEMFUN
	#define UCFG_INTRINSIC_MEMFUN 1
#endif

#define UCFG_USE_RESOURCES 0
#define UCFG_BITCOIN_USE_CRASHDUMP 0

#define UCFG_BITCOIN_THERMAL_CONTROL 0

#define UCFG_COIN_GENERATE 1

#define UCFG_COIN_DOGECOIN 0
#define UCFG_COIN_MOMENTUM 0
#define UCFG_COIN_PRIME 0

#define UCFG_IMP_GROESTL 'T'

#define UCFG_SPH_SMALL_FOOTPRINT 0

#define UCFG_COIN_BIP0014_USER_AGENT "/Groestlcoin-wpf:2.19.0.7/"

#define UCFG_OS_IMPTLS 1

#define UCFG_COIN_DEFAULT_PORT 1331
