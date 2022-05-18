namespace Blockcore.Dns
{
    using NBitcoin;
    using NBitcoin.Crypto;
    using NBitcoin.DataEncoders;
    using System.Text;
    using System.Text.Json;

    public class IdentityService
    {
        private readonly ILogger<IdentityService> logger;

        public IdentityService(ILogger<IdentityService> logger)
        {
            this.logger = logger;
        }

        public bool VerifyIdentity(DnsRequest dnsRequest, DnsSettings dnsSettings)
        {
            foreach (var identity in dnsSettings.Identities)
            {
                if (identity == dnsRequest.Auth.Identity)
                {
                    var created = new DateTime(dnsRequest.Data.Ticks);

                    if ((DateTime.UtcNow - created).Minutes > 1)
                    {
                        return false;
                    }

                    var pubkeyString = identity.Substring(identity.IndexOf("did:is:"));
                    PubKey pubKey = new PubKey(Encoders.Hex.DecodeData(pubkeyString));

                    var serialized = JsonSerializer.Serialize(dnsRequest.Data);
                    var bytes = Encoding.UTF8.GetBytes(serialized);
                    var hashed = Hashes.Hash256(bytes);
                    var signature = ECDSASignature.FromDER(Encoders.Hex.DecodeData(dnsRequest.Auth.Signature));

                    var success =  pubKey.Verify(hashed, signature);

                    return success;
                }
            }

            return false;
        }

        public void CreateIdentity(DnsRequest dnsRequest, AgentSettings agentSettings)
        {
            var key = new Key(Encoders.Hex.DecodeData(agentSettings.Secret));

            dnsRequest.Data.Ticks = DateTime.UtcNow.Ticks;

            var serialized = JsonSerializer.Serialize(dnsRequest.Data);
            var bytes = Encoding.UTF8.GetBytes(serialized);
            var hashed = Hashes.Hash256(bytes);
            var signed = key.Sign(hashed);
            var siganture = Encoders.Hex.EncodeData(signed.ToDER());
            var identity = $"did:is:{ Encoders.Hex.EncodeData(key.PubKey.ToBytes()) }";

            dnsRequest.Auth = new DnsAuth
            {
                Signature = siganture,
                Identity = identity
            };
        }
    }
}
