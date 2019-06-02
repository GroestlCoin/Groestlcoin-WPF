#	Build MSI packages

WIX_LINK = light.exe -ext WiXNetFxExtension -ext WixUIExtension -ext WixUtilExtension -dWixUILicenseRtf=doc\gplv3.rtf

msi : ../../groestlcoin-wpf_x64.msi # ../../groestlcoin-wpf_x86.msi

../../groestlcoin-wpf_x86.msi : groestlcoin-wpf.wxs ..\..\x86_R_St\Groestlcoin-WPF.exe ..\..\x86_R_St\coineng.dll ..\..\x86_R_St\groestlcoin-miner.exe ..\..\miner-example.cmd help\groestlcoin-wpf.htm
	candle.exe -o groestlcoin-wpf-x86.wixobj groestlcoin-wpf.wxs
	$(WIX_LINK)  -out $@ groestlcoin-wpf-x86.wixobj

../../groestlcoin-wpf_x64.msi : groestlcoin-wpf.wxs ..\..\x64_R_St\Groestlcoin-WPF.exe ..\..\x64_R_St\coineng.dll  help\groestlcoin-wpf.htm # ..\..\x64_R_St\groestlcoin-miner.exe ..\..\miner-example.cmd
	candle.exe -arch x64 -o groestlcoin-wpf-x64.wixobj groestlcoin-wpf.wxs
	$(WIX_LINK) -out $@ groestlcoin-wpf-x64.wixobj
