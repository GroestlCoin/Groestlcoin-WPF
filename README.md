Groestlcoin-WPF Wallet & Command-line Miner
============================================================

Build for Windows by Visual Studio 2015 or 2013 instructions
------------------------------------------------------------
<pre>
git clone --recursive https://github.com/GroestlCoin/Groestlcoin-WPF
cd Groestlcoin-WPF
msbuild groestlcoin.sln /p:Configuration=R_St,Platform=x86
  <i>or</i>
msbuild groestlcoin.sln /p:Configuration=R_St,Platform=x64
</pre>


Miner Command-Line Manual
--------------------------
<pre>
groestlcoin-miner {-options}
  Options:
    -a groestlcoin |<seconds>   hashing algorithm (groestl), or time between getwork requests 1..60, default 15
    -A user-agent       Set custom User-agent string in HTTP header, default: Ufasoft bitcoin miner
    -h                  this help
    -l yes|no           set \'no\' to disable Long-Polling, default \'yes\'\n"
    -o url              in form (http | stratum+tcp | xpt+tcp)://username:password@server.tld:port/path, by default http://127.0.0.1:8332
    -u login            Login to Pool
    -p password         Password to Pool
    -t threads          Number of threads for CPU mining, 0..256, by default is number of CPUs (Cores), 0 - disable CPU mining
    -S                  Install service
    -U                  Uninstall service
	-v                  Verbose output
    -x type=host:port   Use HTTP or SOCKS proxy. Examples: -x http=127.0.0.1:3128, -x socks=127.0.0.1:1080

Example:
  groestlcoin-miner  -t 2  -o stratum+tcp://FtPdckbQRiP1XhNkL78s1LmJBhzYhkZL8V:1@erebor.dwarfpool.com:3345
</pre>
