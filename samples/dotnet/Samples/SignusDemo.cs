﻿using Hyperledger.Indy.PoolApi;
using Hyperledger.Indy.Samples.Utils;
using Hyperledger.Indy.SignusApi;
using Hyperledger.Indy.WalletApi;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Hyperledger.Indy.Samples
{
    static class SignusDemo
    {
        public static async Task Execute()
        {
            Console.WriteLine("Ledger sample -> started");

            var myWalletName = "myWallet";
            var theirWalletName = "theirWallet";

            try
            {
                //1. Create and Open Pool
                await PoolUtils.CreatePoolLedgerConfig();

                //2. Create and Open My Wallet
                await WalletUtils.CreateWalleatAsync(PoolUtils.DEFAULT_POOL_NAME, myWalletName, "default", null, null);

                // 3. Create and Open Trustee Wallet
                await WalletUtils.CreateWalleatAsync(PoolUtils.DEFAULT_POOL_NAME, theirWalletName, "default", null, null);

                //4. Open pool and wallets in using statements to ensure they are closed when finished.
                using (var pool = await Pool.OpenPoolLedgerAsync(PoolUtils.DEFAULT_POOL_NAME, "{}"))
                using (var myWallet = await Wallet.OpenWalletAsync(myWalletName, null, null))
                using (var theirWallet = await Wallet.OpenWalletAsync(theirWalletName, null, null))
                {
                    //5. Create My Did
                    var createMyDidResult = await Signus.CreateAndStoreMyDidAsync(myWallet, "{}");

                    //6. Create Their Did
                    var createTheirDidResult = await Signus.CreateAndStoreMyDidAsync(theirWallet, "{}");
                    var theirDid = createTheirDidResult.Did;
                    var theirVerkey = createTheirDidResult.VerKey;

                    //7. Store Their DID
                    var identityJson = string.Format("{{\"did\":\"{0}\", \"verkey\":\"{1}\"}}", theirDid, theirVerkey);
                    await Signus.StoreTheirDidAsync(myWallet, identityJson);

                    //8. Their sign message
                    var msgBytes = Encoding.UTF8.GetBytes("{\n" +
                            "   \"reqId\":1495034346617224651,\n" +
                            "   \"identifier\":\"GJ1SzoWzavQYfNL9XkaJdrQejfztN4XqdsiV4ct3LXKL\",\n" +
                            "   \"operation\":{\n" +
                            "       \"type\":\"1\",\n" +
                            "       \"dest\":\"4efZu2SXufS556yss7W5k6Po37jt4371RM4whbPKBKdB\"\n" +
                            "   }\n" +
                            "}");

                    var signatureBytes = await Signus.SignAsync(theirWallet, theirDid, msgBytes);

                    //9. Verify message
                    var valid = await Signus.VerifySignatureAsync(myWallet, pool, theirDid, msgBytes, signatureBytes);
                    Debug.Assert(valid == true);

                    //10. Close wallets and pool
                    await myWallet.CloseAsync();
                    await theirWallet.CloseAsync();
                    await pool.CloseAsync();
                }
            }
            finally
            {
                // 12. Delete wallets and Pool ledger config
                await WalletUtils.DeleteWalletAsync(myWalletName, null);
                await WalletUtils.DeleteWalletAsync(theirWalletName, null);
                await PoolUtils.DeletePoolLedgerConfigAsync(PoolUtils.DEFAULT_POOL_NAME);
            }            

            Console.WriteLine("Ledger sample -> completed");
        }
    }
}
