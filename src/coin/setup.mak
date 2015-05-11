#	Build MSI packages

msi : ../../groestlcoin-wpf_x86.msi ../../groestlcoin-wpf_x64.msi

../../groestlcoin-wpf_x86.msi : groestlcoin-wpf.wxs ..\..\x86_R_St\Groestlcoin-WPF.exe ..\..\x86_R_St\coineng.dll ..\..\x86_R_St\groestlcoin-miner.exe ..\..\miner-example.cmd help\groestlcoin-wpf.htm
	candle.exe groestlcoin-wpf.wxs
	light.exe -ext WiXNetFxExtension -ext WixUIExtension -ext WixUtilExtension  -out $@ groestlcoin-wpf.wixobj

../../groestlcoin-wpf_x64.msi : groestlcoin-wpf.wxs ..\..\x64_R_St\Groestlcoin-WPF.exe ..\..\x64_R_St\coineng.dll ..\..\x64_R_St\groestlcoin-miner.exe ..\..\miner-example.cmd help\groestlcoin-wpf.htm
	candle.exe -arch x64 groestlcoin-wpf.wxs
	light.exe -ext WiXNetFxExtension -ext WixUIExtension -ext WixUtilExtension -out $@ groestlcoin-wpf.wixobj


