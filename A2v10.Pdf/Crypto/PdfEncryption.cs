// Copyright © 2018-2020 Alex Kukhtin. All rights reserved.


using System;
using System.IO;
using System.Security.Cryptography;

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
			0x28, 0xBF, 0x4E, 0x5E, 0x4E, 0x75, 0x8A, 0x41, 0x64, 0x00, 0x4E, 0x56, 0xFF, 0xFA, 0x01, 0x08, 0x2E, 0x2E,
			0x00, 0xB6, 0xD0, 0x68, 0x3E, 0x80, 0x2F, 0x0C, 0xA9, 0xFE, 0x64, 0x53, 0x69, 0x7A
		};


		private static readonly Byte[] _metadataPad = {
			0xff, 0xff, 0xff, 0xff
		};

		private Byte[] _mkey = Array.Empty<Byte>();
		private Byte[] _ownerKey = new Byte[32];
		private readonly Byte[] _userKey = new Byte[32];


		public Byte[] UserKey => _userKey;

		public PdfEncryption()
		{
			_revision = AlgRevision.STANDARD_ENCRYPTION_40;
			_keyLength = 40;
			_keySize = 0;
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


		virtual public void SetupByOwnerPassword(Byte[] documentID, Byte[] userKey,
			Byte[] ownerKey, Int64 permissions)
		{
			SetupByOwnerPad(documentID, PadPassword(), userKey, ownerKey, permissions);
		}

		Byte[] PadPassword()
		{
			Byte[] userPad = new Byte[32];
			Array.Copy(_pad, 0, userPad, 0, 32);
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

		virtual public void SetupByUserPassword(Byte[] documentId, Byte[] ownerKey, Int64 permissions)
		{
			SetupByUserPad(_documentId, PadPassword(), ownerKey, permissions);
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
				var md5 = new MD5();
				md5.BlockUpdate(_pad, 0, _pad.Length);
				md5.BlockUpdate(_documentId, 0, _documentId.Length);
				Byte[] digest = md5.DoFinal();

				Array.Copy(digest, 0, _userKey, 0, 16);
				for (Int32 k = 16; k < 32; ++k)
					_userKey[k] = 0;
				for (Int32 i = 0; i < 20; ++i)
				{
					for (Int32 j = 0; j < _mkey.Length; ++j)
						digest[j] = (Byte)(_mkey[j] ^ i);

					RC4.Encrypt(digest, _userKey, 0, _mkey.Length, 16);
				}
			}
			else
			{
				RC4.Encrypt(_pad, _userKey, 0);
			}
		}

		private Byte[] ComputeOwnerKey(Byte[] userPad, Byte[] ownerPad)
		{
			Byte[] ownerKey = new Byte[32];
			Byte[] digest = MD5.Digest(ownerPad);
			if (_revision == AlgRevision.STANDARD_ENCRYPTION_128 || _revision == AlgRevision.AES_128)
			{
				Byte[] mkey = new Byte[_keyLength / 8];

				for (Int32 k = 0; k < 50; ++k)
					Array.Copy(MD5.Digest(digest, 0, mkey.Length), 0, digest, 0, mkey.Length);

				Array.Copy(userPad, 0, ownerKey, 0, 32);

				for (Int32 i = 0; i < 20; ++i)
				{
					for (Int32 j = 0; j < mkey.Length; ++j)
						mkey[j] = (Byte)(digest[j] ^ i);

					RC4.Encrypt(mkey, ownerKey, 0);
				}
			}
			else
			{
				var rc4 = new RC4(digest, 0, 5);
				rc4.Encrypt(userPad, ownerKey);
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
			var md5 = new MD5();
			md5.BlockUpdate(userPad, 0, userPad.Length);
			md5.BlockUpdate(ownerKey, 0, ownerKey.Length);

			Byte[] ext = new Byte[4];
			ext[0] = (Byte)permissions;
			ext[1] = (Byte)(permissions >> 8);
			ext[2] = (Byte)(permissions >> 16);
			ext[3] = (Byte)(permissions >> 24);
			md5.BlockUpdate(ext, 0, 4);
			if (_documentId != null)
				md5.BlockUpdate(_documentId, 0, _documentId.Length);
			if (!_encryptMetadata)
				md5.BlockUpdate(_metadataPad, 0, _metadataPad.Length);
			Byte[] hash = md5.DoFinal();

			Byte[] digest = new Byte[_mkey.Length];
			Array.Copy(hash, 0, digest, 0, _mkey.Length);

			// only use the really needed bits as input for the hash
			if (_revision == AlgRevision.STANDARD_ENCRYPTION_128 || _revision == AlgRevision.AES_128)
			{
				for (Int32 k = 0; k < 50; ++k)
					Array.Copy(MD5.Digest(digest), 0, digest, 0, _mkey.Length);
			}
			Array.Copy(digest, 0, _mkey, 0, _mkey.Length);
		}


		public Byte[] DecryptByteArray(Byte[] b)
		{
			MemoryStream ba = new MemoryStream();

			StandardDecryption dec = new StandardDecryption(_key, _revision);

			Byte[] b2 = dec.Update(b, 0, b.Length);
			if (b2 != null)
				ba.Write(b2, 0, b2.Length);

			return ba.ToArray();
		}

		virtual public void SetHashKey(Int32 number, Int32 generation)
		{
			if (_revision == AlgRevision.AES_256)
				return;

			Byte[] extra = new Byte[5];

			var md5 = new MD5();

			extra[0] = (Byte)number;
			extra[1] = (Byte)(number >> 8);
			extra[2] = (Byte)(number >> 16);
			extra[3] = (Byte)generation;
			extra[4] = (Byte)(generation >> 8);
			md5.BlockUpdate(_mkey, 0, _mkey.Length);
			md5.BlockUpdate(extra, 0, extra.Length);
			if (_revision == AlgRevision.AES_128)
				md5.BlockUpdate(_salt, 0, _salt.Length);
			_key = md5.DoFinal();

			_keySize = _mkey.Length + 5;
			if (_keySize > 16)
				_keySize = 16;
		}
	}
}
