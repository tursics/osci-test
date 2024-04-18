using System.IO;
using System.Security.Cryptography;

/*
THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS
OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED.  IN NO EVENT SHALL THE AUTHOR OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
SUCH DAMAGE.
*/
namespace Osci.Common
{
    /// <summary>A utility class which wraps around any given OutputStream and encodes the
    /// data before writing to the outputstream as per the BASE64 encoding standards.
    /// <p>Author: Gokul Singh, gokulsingh@123india.com</p>
    /// </summary>
    public class Base64OutputStream
        : CryptoStream
    {
        /// <summary>Flag to denote if the flush denotes the end of the stream
        /// for Encoding and padding if required should be done.
        /// </summary>
        private readonly bool _isFlushEnd;

        /// <summary>It takes the output stream around which it wraps and the
        /// behaviour of the stream when flush is called upon this stream.
        /// </summary>
        /// <param name="stream">Outputstream around which it wraps.
        /// </param>
        /// <param name="isFlushEnd">Flag denoting if the <code>flush </code> method
        /// denotes end of stream.
        /// </param>
        public Base64OutputStream(Stream stream, bool isFlushEnd = true) 
            : base(new SplitStream(stream), new ToBase64Transform(), CryptoStreamMode.Write)
        {
            _isFlushEnd = isFlushEnd;
        }

        /// <summary>
        /// It flushes the underlying stream. The behaviour of this method depends upon the
        /// flag <code>isFlushEnd</code> If the flag is set then call to this method is taken
        /// to indicate that no more data is to written to this stream with base64 encoding
        /// and hence padding is done if required as per base64 rules. If the flag is not set,
        /// then it just flushes the underlying stream.
        /// </summary>
        public override void Flush()
        {
            if (_isFlushEnd)
            {
                FlushFinalBlock();
            }
            else
            {
                base.Flush();
            }
        }
    }
}