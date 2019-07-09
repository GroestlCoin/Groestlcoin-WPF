/*######   Copyright (c) 2015-2019 Ufasoft  http://ufasoft.com  mailto:support@ufasoft.com,  Sergey Pavlov  mailto:dev@ufasoft.com ####
#                                                                                                                                     #
# 		See LICENSE for licensing information                                                                                         #
#####################################################################################################################################*/

#pragma once

namespace Coin {

const unsigned
    MAX_BLOCK_WEIGHT = 4000000,  // BIP141
    MAX_BLOCK_SIGOPS_COST = MAX_BLOCK_WEIGHT / 50;

const int WITNESS_SCALE_FACTOR = 4;
const unsigned MAX_BLOCK_SIZE = MAX_BLOCK_WEIGHT / WITNESS_SCALE_FACTOR; //!!!R
const int MAX_BLOCK_SIZE_GEN = MAX_BLOCK_SIZE / 2;
const int MAX_STANDARD_TX_SIZE = 100000;
const int MAX_FREE_TRANSACTION_CREATE_SIZE = 1000;
const int MAX_FUTURE_SECONDS = 7200;
const size_t MAX_ORPHAN_TRANSACTIONS = MAX_BLOCK_SIZE / 100;
const size_t MAX_SCRIPT_ELEMENT_SIZE = 520;
const unsigned MAX_SCRIPT_SIZE = 10000;


// Standard behavior for acception into Tx pool

const int32_t MAX_STANDARD_VERSION = 2;

const unsigned
	MAX_STANDARD_SIGSCRIPT_SIZE = 1650,		// enough for 15-in-16 MultiSig
    MAX_STANDARD_TX_WEIGHT = 400000;

const unsigned DUST_RELAY_TX_FEE = 3000;



} // Coin::

