/*######   Copyright (c) 2019      Ufasoft  http://ufasoft.com  mailto:support@ufasoft.com,  Sergey Pavlov  mailto:dev@ufasoft.com ####
#                                                                                                                                     #
# 		See LICENSE for licensing information                                                                                         #
#####################################################################################################################################*/

#include <el/ext.h>

#include "eng.h"

namespace Coin {

void PruneDbThread::Execute() {
	Name = "PruneThread";
	CCoinEngThreadKeeper engKeeper(&Eng);
	for (int h = From; h <= To; ++h) {
		if (m_bStop)
			return;
		CoinEngTransactionScope scopeBlockSavepoint(Eng);
		for (auto& tx : Eng.Db->FindBlock(h).Txes)
			for (auto& txIn : tx.TxIns())
				Eng.Db->PruneTxo(txIn.PrevOutPoint, h);
		Eng.Db->SetLastPrunedHeight(h);
	}
	TRC(1, "Pruned spent TXOs upto Block " << To)
}

void CoinEng::TryStartPruning() {
	if (Mode == EngMode::Bootstrap) {
		auto lastPrunedHeight = Db->GetLastPrunedHeight();
		auto bestHeight = BestBlockHeight();
		if (lastPrunedHeight + PRUNE_UPTO_LAST_BLOCKS < bestHeight)
			(new PruneDbThread(_self, lastPrunedHeight + 1, bestHeight - PRUNE_UPTO_LAST_BLOCKS))->Start();
	}
}

} // Coin::
