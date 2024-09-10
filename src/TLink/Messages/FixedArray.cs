//  DSC TLink - a communications library for DSC Powerseries NEO alarm panels
//  Copyright (C) 2024 Brian Humlicek

//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.

//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.

namespace DSC.TLink.Messages
{
    internal class FixedArray : BinaryMessage.FieldMetadata<byte[]>
    {
        int length;
        public FixedArray(int length)
        {
            this.length = length;
        }
        public override int Length => length;
        protected override IEnumerable<byte> GetFieldBytes()
        {
            for (int i = 0; i < length; i++)
            {
                if (initializationBuffer != null && i < initializationBuffer.Length)
                {
                    yield return initializationBuffer[i];
                }
                yield return 0;
            }
        }
        protected override byte[] GetPropertyValue(byte[] messageBytes)
        {
            byte[] result = new byte[Length];
            Array.Copy(messageBytes, Offset, result, 0, Length);
            return result;
        }
    }
}
