#	Build MSI packages


../../groestlcoin-wpf_x86.msi : groestlcoin-wpf.wxs
	c:/util/wix/candle groestlcoin-wpf.wxs
	c:/util/wix/light -out $@ groestlcoin-wpf.wixobj




# ../../groestlcoin-wpf_x86.msi : groestlcoin-wpf.wxs
#	c:/util/wix/candle groestlcoin-wpf.wxs
#	c:/util/wix/light -out $@ groestlcoin-wpf.wixobj


