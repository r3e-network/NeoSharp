# NeoSharp

A comprehensive C# library for Neo blockchain development, converted from NeoSwift.

## Features

- Full implementation of Neo RPC API
- Support for NEP-17 (fungible tokens) and NEP-11 (non-fungible tokens)
- Wallet management and account operations
- Smart contract interaction and deployment
- Transaction building and signing
- Cryptographic operations (ECDSA, Hash functions)
- Type-safe contract parameters
- Async/await support throughout
- Cross-platform compatibility (.NET 8.0+)

## Installation

Install via NuGet:

```bash
dotnet add package NeoSharp
```

Or add to your `.csproj`:

```xml
<PackageReference Include="NeoSharp" Version="1.0.0" />
```

## Quick Start

### Connect to Neo Node

```csharp
using NeoSharp.Protocol;
using NeoSharp.Types;

// Connect to Neo MainNet
var neo = new NeoSharp("http://seed1.neo.org:10332");

// Or connect to TestNet
var config = new NeoSharpConfig().UseTestNet();
var testNet = new NeoSharp("http://seed1.ngd.network:20332", config);
```

### Basic Operations

```csharp
// Get current block count
var blockCount = await neo.GetBlockCountAsync();
Console.WriteLine($"Current block height: {blockCount}");

// Get a block by index
var block = await neo.GetBlockAsync(1000000);
Console.WriteLine($"Block hash: {block.Hash}");

// Get NEO balance
var neoHash = Hash160.Parse("0xef4073a0f2b305a38ec4050e4d3d28bc40ea63f5");
var address = Hash160.Parse("YOUR_ADDRESS_SCRIPT_HASH");
var balances = await neo.GetNep17BalancesAsync(address);
```

### Smart Contract Interaction

```csharp
// Invoke a smart contract function
var contractHash = Hash160.Parse("0xef4073a0f2b305a38ec4050e4d3d28bc40ea63f5"); // NEO token
var result = await neo.InvokeFunctionAsync(
    contractHash,
    "symbol",
    new List<ContractParameter>()
);

if (result.State == "HALT")
{
    Console.WriteLine($"Symbol: {result.Stack[0].Value}");
}

// Transfer NEO tokens
var from = new Signer { Account = yourAddress, Scopes = WitnessScope.CalledByEntry };
var transferParams = new List<ContractParameter>
{
    ContractParameter.Hash160(yourAddress),
    ContractParameter.Hash160(recipientAddress),
    ContractParameter.Integer(100), // Amount in NEO
    ContractParameter.Any(null)
};

var transferResult = await neo.InvokeFunctionAsync(
    contractHash,
    "transfer",
    transferParams,
    new List<Signer> { from }
);
```

### Wallet Operations

```csharp
using NeoSharp.Wallet;
using NeoSharp.Crypto;

// Create a new account
var privateKey = new ECPrivateKey();
var account = new Account(privateKey);
Console.WriteLine($"Address: {account.Address}");
Console.WriteLine($"Private Key (WIF): {privateKey.ToWIF()}");

// Import account from WIF
var wif = "YOUR_PRIVATE_KEY_WIF";
var importedKey = ECPrivateKey.FromWIF(wif);
var importedAccount = new Account(importedKey);

// Create wallet
var wallet = new Wallet("MyWallet");
wallet.AddAccount(account);
```

### Transaction Building

```csharp
using NeoSharp.Transaction;
using NeoSharp.Script;

// Build a transaction
var builder = new TransactionBuilder(neo);
builder.SetScript(scriptBuilder =>
{
    scriptBuilder.EmitDynamicCall(contractHash, "transfer", 
        yourAddress, recipientAddress, 100, null);
});
builder.AddSigner(new Signer 
{ 
    Account = yourAddress, 
    Scopes = WitnessScope.CalledByEntry 
});

var tx = await builder.BuildAsync();

// Sign transaction
tx.Sign(account);

// Send transaction
var result = await neo.SendRawTransactionAsync(tx.ToArray().ToHexString());
Console.WriteLine($"Transaction sent: {result.Hash}");
```

## Advanced Features

### NEP-17 Token Operations

```csharp
// Get all NEP-17 balances for an address
var nep17Balances = await neo.GetNep17BalancesAsync(address);
foreach (var balance in nep17Balances.Balance)
{
    Console.WriteLine($"{balance.Symbol}: {balance.Amount} ({balance.Name})");
}

// Get transfer history
var transfers = await neo.GetNep17TransfersAsync(address);
foreach (var transfer in transfers.Sent)
{
    Console.WriteLine($"Sent {transfer.Amount} to {transfer.TransferAddress}");
}
```

### NEP-11 NFT Operations

```csharp
// Get NFT balances
var nftBalances = await neo.GetNep11BalancesAsync(address);
foreach (var nft in nftBalances.Balance)
{
    Console.WriteLine($"NFT Contract: {nft.AssetHash}");
    foreach (var token in nft.Tokens)
    {
        Console.WriteLine($"  Token ID: {token.TokenId}, Amount: {token.Amount}");
    }
}

// Get NFT properties
var nftContract = Hash160.Parse("NFT_CONTRACT_HASH");
var tokenId = "TOKEN_ID";
var properties = await neo.GetNep11PropertiesAsync(nftContract, tokenId);
```

### State Service

```csharp
// Get state root
var stateRoot = await neo.GetStateRootAsync(blockIndex);
Console.WriteLine($"State root hash: {stateRoot.RootHash}");

// Get storage value with proof
var proof = await neo.GetProofAsync(stateRoot.RootHash, contractHash, "storageKey");
```

## Configuration

```csharp
// Custom configuration
var config = new NeoSharpConfig
{
    BlockInterval = 15000, // milliseconds
    PollingInterval = 1000, // milliseconds
    MaxValidUntilBlockIncrement = 5760,
    NetworkMagic = 860833102, // MainNet
    AddressVersion = 0x35,
    AllowTransmissionOnFault = false
};

// Use predefined networks
config.UseMainNet();  // or
config.UseTestNet();  // or
config.UseCustomNet(customMagicNumber);
```

## Error Handling

```csharp
try
{
    var result = await neo.InvokeFunctionAsync(contractHash, "method");
    if (result.State == "FAULT")
    {
        Console.WriteLine($"Execution failed: {result.Exception}");
    }
}
catch (JsonRpcException ex)
{
    Console.WriteLine($"RPC error: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

## Requirements

- .NET 8.0 or later
- Neo node RPC endpoint

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

This library is a C# port of the [NeoSwift](https://github.com/crisogray/NeoSwift) library. Special thanks to the original authors and contributors.

## Support

For questions and support, please open an issue on the [GitHub repository](https://github.com/neo-project/NeoSwift).