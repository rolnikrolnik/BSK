namespace bskpreview
{
    internal class RSAPublicKey
    {
        public string Username { get; set; }
        public string PathToKey { get; set; }

        public override string ToString()
        {
            return this.Username;
        }
    }
}
