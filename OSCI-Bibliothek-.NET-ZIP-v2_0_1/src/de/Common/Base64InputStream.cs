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

using System.IO;
using System.Security.Cryptography;

namespace Osci.Common
{
    /// <summary>A utility class which wraps around any given inputstream and decodes the
    /// inputstream as per the BASE64 encoding standards.
    /// <p>This class reads data from the underlying inputstream in chunks of four.</p>
    /// <p>Author: Gokul Singh, gokulsingh@123india.com</p>
    /// </summary>
    public class Base64InputStream 
        : CryptoStream
    {
        /// <summary>Constructor which takes the input stream around which it wraps.
        /// </summary>
        /// <param name="in_Renamed">Stream around which this wraps.
        /// </param>
        /// 
        public Base64InputStream(Stream stream) 
            : base(new BufferedStream(stream), new FromBase64Transform(), CryptoStreamMode.Read)
        {
        }
    }
}