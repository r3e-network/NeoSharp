using System;
using System.Threading.Tasks;
using NeoSharp.Protocol;
using NeoSharp.Types;
using NeoSharp.Wallet;
using NeoSharp.Crypto;

namespace NeoSharp.Examples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("NeoSharp SDK Examples");
            Console.WriteLine("======================\n");

            // Example 1: Connect to Neo TestNet
            await ConnectToNeoExample();

            // Example 2: Query blockchain data
            await QueryBlockchainExample();

            // Example 3: Create and manage accounts
            await AccountManagementExample();

            // Example 4: Query NEP-17 token balances
            await QueryNep17BalancesExample();

            // Example 5: Invoke smart contract
            await InvokeContractExample();

            Console.WriteLine("\nExamples completed!");
        }

        static async Task ConnectToNeoExample()
        {
            Console.WriteLine("Example 1: Connecting to Neo TestNet");
            Console.WriteLine("------------------------------------");

            try
            {
                // Configure for TestNet
                var config = new NeoSharpConfig().UseTestNet();
                var neo = new NeoSharp("http://seed1.ngd.network:20332", config);

                // Get version information
                var version = await neo.GetVersionAsync();
                Console.WriteLine($"Connected to Neo node version: {version.UserAgent}");
                Console.WriteLine($"Protocol version: {version.Protocol}");
                Console.WriteLine($"Network: {version.Network}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}\n");
            }
        }

        static async Task QueryBlockchainExample()
        {
            Console.WriteLine("Example 2: Query Blockchain Data");
            Console.WriteLine("--------------------------------");

            try
            {
                var neo = new NeoSharp("http://seed1.ngd.network:20332");

                // Get current block count
                var blockCount = await neo.GetBlockCountAsync();
                Console.WriteLine($"Current block height: {blockCount}");

                // Get latest block
                var latestBlock = await neo.GetBlockAsync(blockCount - 1);
                Console.WriteLine($"Latest block hash: {latestBlock.Hash}");
                Console.WriteLine($"Latest block time: {latestBlock.DateTime}");
                Console.WriteLine($"Transactions in block: {latestBlock.Transactions?.Count ?? 0}");

                // Get committee members
                var committee = await neo.GetCommitteeAsync();
                Console.WriteLine($"Committee members: {committee.Count}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}\n");
            }
        }

        static Task AccountManagementExample()
        {
            Console.WriteLine("Example 3: Account Management");
            Console.WriteLine("-----------------------------");

            try
            {
                // Create a new account
                var privateKey = new ECPrivateKey();
                var account = new Account(privateKey);

                Console.WriteLine("New account created:");
                Console.WriteLine($"Address: {account.Address}");
                Console.WriteLine($"Script Hash: {account.GetScriptHash()}");
                Console.WriteLine($"Private Key (WIF): {privateKey.ToWIF()}");
                Console.WriteLine($"Public Key: {account.GetPublicKey().GetEncoded(true).ToHexString()}");

                // Create wallet and add account
                var wallet = new Wallet("MyTestWallet");
                wallet.AddAccount(account);
                Console.WriteLine($"\nWallet created with name: {wallet.Name}");
                Console.WriteLine($"Accounts in wallet: {wallet.GetAccounts().Count}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}\n");
            }

            return Task.CompletedTask;
        }

        static async Task QueryNep17BalancesExample()
        {
            Console.WriteLine("Example 4: Query NEP-17 Token Balances");
            Console.WriteLine("--------------------------------------");

            try
            {
                var neo = new NeoSharp("http://seed1.ngd.network:20332");

                // Use a known TestNet address with balance
                // This is a public TestNet faucet address
                var address = Hash160.Parse("0x4578060c29f4c03f1e16c84312429d991952c94c");

                var balances = await neo.GetNep17BalancesAsync(address);
                Console.WriteLine($"NEP-17 balances for {address}:");

                if (balances.Balance != null && balances.Balance.Count > 0)
                {
                    foreach (var balance in balances.Balance)
                    {
                        Console.WriteLine($"  {balance.Symbol}: {balance.Amount} ({balance.Name})");
                        Console.WriteLine($"    Contract: {balance.AssetHash}");
                        Console.WriteLine($"    Decimals: {balance.Decimals}");
                        Console.WriteLine($"    Last Updated Block: {balance.LastUpdatedBlock}");
                    }
                }
                else
                {
                    Console.WriteLine("  No NEP-17 tokens found");
                }
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}\n");
            }
        }

        static async Task InvokeContractExample()
        {
            Console.WriteLine("Example 5: Invoke Smart Contract");
            Console.WriteLine("--------------------------------");

            try
            {
                var neo = new NeoSharp("http://seed1.ngd.network:20332");

                // NEO token contract hash
                var neoTokenHash = Hash160.Parse("0xef4073a0f2b305a38ec4050e4d3d28bc40ea63f5");

                // Get NEO token symbol
                Console.WriteLine("Invoking NEO token contract...");
                var symbolResult = await neo.InvokeFunctionAsync(neoTokenHash, "symbol");

                if (symbolResult.State == "HALT")
                {
                    Console.WriteLine($"Execution successful!");
                    Console.WriteLine($"GAS consumed: {symbolResult.GasConsumed}");
                    
                    if (symbolResult.Stack != null && symbolResult.Stack.Count > 0)
                    {
                        var symbolValue = symbolResult.Stack[0].Value?.ToString() ?? "";
                        // Convert base64 to string if needed
                        if (symbolResult.Stack[0].Type == "ByteString" && !string.IsNullOrEmpty(symbolValue))
                        {
                            try
                            {
                                var bytes = Convert.FromBase64String(symbolValue);
                                symbolValue = System.Text.Encoding.UTF8.GetString(bytes);
                            }
                            catch { }
                        }
                        Console.WriteLine($"NEO Token Symbol: {symbolValue}");
                    }
                }
                else
                {
                    Console.WriteLine($"Execution failed with state: {symbolResult.State}");
                    if (!string.IsNullOrEmpty(symbolResult.Exception))
                    {
                        Console.WriteLine($"Exception: {symbolResult.Exception}");
                    }
                }

                // Get total supply
                var supplyResult = await neo.InvokeFunctionAsync(neoTokenHash, "totalSupply");
                if (supplyResult.State == "HALT" && supplyResult.Stack != null && supplyResult.Stack.Count > 0)
                {
                    var supply = supplyResult.Stack[0].Value?.ToString() ?? "0";
                    Console.WriteLine($"NEO Total Supply: {supply}");
                }

                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}\n");
            }
        }
    }
}