// Copyright © 2018-2020 Alex Kukhtin. All rights reserved.

/*
    Uses iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation with the addition of the
    following permission added to Section 15 as permitted in Section 7(a):
    FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
    ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
    OF THIRD PARTY RIGHTS
    
    This program is distributed in the hope that it will be useful, but
    WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
    or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU Affero General Public License for more details.
    You should have received a copy of the GNU Affero General Public License
    along with this program; if not, see http://www.gnu.org/licenses or write to
    the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
    Boston, MA, 02110-1301 USA, or download the license from the following URL:
    http://itextpdf.com/terms-of-use/
    
    The interactive user interfaces in modified source and object code versions
    of this program must display Appropriate Legal Notices, as required under
    Section 5 of the GNU Affero General Public License.
    
    In accordance with Section 7(b) of the GNU Affero General Public License,
    a covered work must retain the producer line in every PDF that is created
    or manipulated using iText.
    
    You can be released from the requirements of the license by purchasing
    a commercial license. Buying such a license is mandatory as soon as you
    develop commercial activities involving the iText software without
    disclosing the source code of your own applications.
    These activities include: offering paid services to customers as an ASP,
    serving PDFs on the fly in a web application, shipping iText with a closed
    source product.
    
    For more information, please contact iText Software Corp. at this
    address: sales@itextpdf.com
 */

using System;
using System.IO;

namespace A2v10.Pdf.Crypto
{
	public enum AlgRevision
	{
		STANDARD_ENCRYPTION_40 = 2,
		STANDARD_ENCRYPTION_128 = 3,
		AES_128 = 4,
		AES_256 = 5
	}

	public class PdfEncryption
	{
		private Int32 _keyLength;
		private AlgRevision _revision;
		private Int32 _keySize;
		private Byte[] _documentId;
		private Int64 _permissions;
		private Boolean _encryptMetadata;
		private Byte[] _key;

		private static readonly Byte[] _salt = { 0x73, 0x41, 0x6c, 0x54 };

		private static readonly Byte[] _pad = {
			0x28, 0xBF, 0x4E, 0x5E, 0x4E, 0x75,
			0x8A, 0x41, 0x64, 0x00, 0x4E, 0x56,
			0xFF, 0xFA, 0x01, 0x08, 0x2E, 0x2E,
			0x00, 0xB6, 0xD0, 0x68, 0x3E, 0x80,
			0x2F, 0x0C, 0xA9, 0xFE, 0x64, 0x53,
			0x69, 0x7A
		};


		private static readonly Byte[] _metadataPad = {
			255, 255, 255, 255
		};

		private Byte[] _mkey = Array.Empty<Byte>();
		private Byte[] _ownerKey = new Byte[32];
		private readonly Byte[] _userKey = new Byte[32];

		private IDigest _md5;

		private EncryptArc4 _rc4 = new EncryptArc4();

		public Byte[] UserKey => _userKey;

		public PdfEncryption()
		{
			_revision = AlgRevision.STANDARD_ENCRYPTION_40;
			_keyLength = 40;
			_keySize = 0;
			_md5 = DigestAlgorithms.GetDigest("MD5");
		}

		virtual public void SetCryptoMode(AlgRevision revision, Int32 keyLength)
		{
			//encryptMetadata = (mode & PdfWriter.DO_NOT_ENCRYPT_METADATA) != PdfWriter.DO_NOT_ENCRYPT_METADATA;
			//embeddedFilesOnly = (mode & PdfWriter.EMBEDDED_FILES_ONLY) == PdfWriter.EMBEDDED_FILES_ONLY;
			//mode &= PdfWriter.ENCRYPTION_MASK;
			_encryptMetadata = true;
			_revision = revision;
			switch (_revision)
			{
				case AlgRevision.STANDARD_ENCRYPTION_40:
					_encryptMetadata = true;
					//embeddedFilesOnly = false;
					_keyLength = 40;
					break;
				case AlgRevision.STANDARD_ENCRYPTION_128:
					//embeddedFilesOnly = false;
					if (keyLength > 0)
						_keyLength = keyLength;
					else
						_keyLength = 128;
					break;
				case AlgRevision.AES_128:
					_keyLength = 128;
					break;
				case AlgRevision.AES_256:
					_keyLength = 256;
					_keySize = 32;
					break;
				default:
					throw new ArgumentException("No valid encryption.mode");
			}
		}


		virtual public void SetupByOwnerPassword(Byte[] documentID, Byte[] ownerPassword, Byte[] userKey,
			Byte[] ownerKey, Int64 permissions)
		{
			SetupByOwnerPad(documentID, PadPassword(ownerPassword), userKey, ownerKey, permissions);
		}

		Byte[] PadPassword(Byte[] userPassword)
		{
			Byte[] userPad = new Byte[32];
			if (userPassword == null)
				Array.Copy(_pad, 0, userPad, 0, 32);
			else
			{
				Array.Copy(userPassword, 0, userPad, 0, Math.Min(userPassword.Length, 32));
				if (userPassword.Length < 32)
					Array.Copy(_pad, 0, userPad, userPassword.Length, 32 - userPassword.Length);
			}
			return userPad;
		}

		void SetupByOwnerPad(Byte[] documentID, Byte[] ownerPad, Byte[] userKey, Byte[] ownerKey,
			Int64 permissions)
		{
			//userPad will be set in this.ownerKey
			Byte[] userPad = ComputeOwnerKey(ownerKey, ownerPad); 
			SetupGlobalEncryptionKey(documentID, userPad, ownerKey, permissions); //step 3
			SetupUserKey();
		}

		virtual public void SetupByUserPassword(Byte[] documentId, Byte[] userPassword, Byte[] ownerKey, Int64 permissions)
		{
			SetupByUserPad(_documentId, PadPassword(userPassword), ownerKey, permissions);
		}

		private void SetupByUserPad(Byte[] documentId, Byte[] userPad, Byte[] ownerKey, Int64 permissions)
		{
			SetupGlobalEncryptionKey(documentId, userPad, ownerKey, permissions);
			SetupUserKey();
		}


		void SetupUserKey()
		{
			if (_revision == AlgRevision.STANDARD_ENCRYPTION_128 || _revision == AlgRevision.AES_128)
			{
				_md5.BlockUpdate(_pad, 0, _pad.Length);
				_md5.BlockUpdate(_documentId, 0, _documentId.Length);
				Byte[] digest = new Byte[_md5.GetDigestSize()];
				_md5.DoFinal(digest, 0);
				_md5.Reset();
				Array.Copy(digest, 0, _userKey, 0, 16);
				for (Int32 k = 16; k < 32; ++k)
					_userKey[k] = 0;
				for (Int32 i = 0; i < 20; ++i)
				{
					for (Int32 j = 0; j < _mkey.Length; ++j)
						digest[j] = (Byte)(_mkey[j] ^ i);
					_rc4.PrepareARC4Key(digest, 0, _mkey.Length);
					_rc4.EncryptARC4(_userKey, 0, 16);
				}
			}
			else
			{
				_rc4.PrepareARC4Key(_mkey);
				_rc4.EncryptARC4(_pad, _userKey);
			}
		}

		private Byte[] ComputeOwnerKey(Byte[] userPad, Byte[] ownerPad)
		{
			Byte[] ownerKey = new Byte[32];

			Byte[] digest = DigestAlgorithms.Digest("MD5", ownerPad);
			if (_revision == AlgRevision.STANDARD_ENCRYPTION_128 || _revision == AlgRevision.AES_128)
			{
				Byte[] mkey = new Byte[_keyLength / 8];
				// only use for the input as many bit as the key consists of
				for (Int32 k = 0; k < 50; ++k)
					Array.Copy(DigestAlgorithms.Digest("MD5", digest, 0, mkey.Length), 0, digest, 0, mkey.Length);
				Array.Copy(userPad, 0, ownerKey, 0, 32);
				for (Int32 i = 0; i < 20; ++i)
				{
					for (Int32 j = 0; j < mkey.Length; ++j)
						mkey[j] = (Byte)(digest[j] ^ i);
					_rc4.PrepareARC4Key(mkey);
					_rc4.EncryptARC4(ownerKey);
				}
			}
			else
			{
				_rc4.PrepareARC4Key(digest, 0, 5);
				_rc4.EncryptARC4(userPad, ownerKey);
			}

			return ownerKey;
		}

		private void SetupGlobalEncryptionKey(Byte[] documentId, Byte[] userPad, Byte[] ownerKey, Int64 permissions)
		{
			_documentId = documentId;
			_ownerKey = ownerKey;
			_permissions = permissions;

			// use variable keylength
			_mkey = new Byte[_keyLength / 8];

			//fixed by ujihara in order to follow PDF refrence
			_md5.Reset();
			_md5.BlockUpdate(userPad, 0, userPad.Length);
			_md5.BlockUpdate(ownerKey, 0, ownerKey.Length);

			Byte[] ext = new Byte[4];
			ext[0] = (Byte)permissions;
			ext[1] = (Byte)(permissions >> 8);
			ext[2] = (Byte)(permissions >> 16);
			ext[3] = (Byte)(permissions >> 24);
			_md5.BlockUpdate(ext, 0, 4);
			if (_documentId != null)
				_md5.BlockUpdate(_documentId, 0, _documentId.Length);
			if (!_encryptMetadata)
				_md5.BlockUpdate(_metadataPad, 0, _metadataPad.Length);
			Byte[] hash = new Byte[_md5.GetDigestSize()];
			_md5.DoFinal(hash, 0);

			Byte[] digest = new Byte[_mkey.Length];
			Array.Copy(hash, 0, digest, 0, _mkey.Length);

			_md5.Reset();

			// only use the really needed bits as input for the hash
			if (_revision == AlgRevision.STANDARD_ENCRYPTION_128 || _revision == AlgRevision.AES_128)
			{
				for (Int32 k = 0; k < 50; ++k)
				{
					Array.Copy(DigestAlgorithms.Digest("MD5", digest), 0, digest, 0, _mkey.Length);
				}
			}
			Array.Copy(digest, 0, _mkey, 0, _mkey.Length);
		}


		virtual public StandardDecryption GetDecryptor()
		{
			return new StandardDecryption(_key, 0, _keySize, _revision);
		}

		virtual public Byte[] DecryptByteArray(Byte[] b)
		{
			MemoryStream ba = new MemoryStream();
			StandardDecryption dec = GetDecryptor();
			Byte[] b2 = dec.Update(b, 0, b.Length);
			if (b2 != null)
				ba.Write(b2, 0, b2.Length);
			b2 = dec.Finish();
			if (b2 != null)
				ba.Write(b2, 0, b2.Length);
			return ba.ToArray();
		}

		virtual public void SetHashKey(Int32 number, Int32 generation)
		{
			if (_revision == AlgRevision.AES_256)
				return;

			Byte[] extra = new Byte[5];

			_md5.Reset(); //added by ujihara
			extra[0] = (Byte)number;
			extra[1] = (Byte)(number >> 8);
			extra[2] = (Byte)(number >> 16);
			extra[3] = (Byte)generation;
			extra[4] = (Byte)(generation >> 8);
			_md5.BlockUpdate(_mkey, 0, _mkey.Length);
			_md5.BlockUpdate(extra, 0, extra.Length);
			if (_revision == AlgRevision.AES_128)
				_md5.BlockUpdate(_salt, 0, _salt.Length);
			_key = new Byte[_md5.GetDigestSize()];
			_md5.DoFinal(_key, 0);
			_md5.Reset();
			_keySize = _mkey.Length + 5;
			if (_keySize > 16)
				_keySize = 16;
		}
	}
}
