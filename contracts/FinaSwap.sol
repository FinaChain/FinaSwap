// SPDX-License-Identifier: MIT
pragma solidity ^0.8.4;

import "@openzeppelin/contracts/access/AccessControl.sol";
import "@openzeppelin/contracts/security/Pausable.sol";

import "./interfaces/IECDSA.sol";
import "./interfaces/IERC20Mintable.sol";
import "./utils/BytesUtils.sol";


contract FinaSwap is Pausable, AccessControl {
    bytes32 public constant ADMIN_ROLE = keccak256("ADMIN_ROLE");
    bytes32 public constant PAUSER_ROLE = keccak256("PAUSER_ROLE");
    
    IECDSA private immutable _ecdsa;
    IERC20Mintable private immutable _token;
    bytes1 private immutable _prefix;

    uint256 private _nativeValue;
    mapping(bytes => uint256) private _balances;

    constructor(IECDSA ecdsa, IERC20Mintable token, bytes1 prefix, uint256 nativeValue)
    {
        _ecdsa = ecdsa;
        _token = token;
        _prefix = prefix;
        _nativeValue = nativeValue;

        _grantRole(ADMIN_ROLE, msg.sender);
        _grantRole(PAUSER_ROLE, msg.sender);
    }

    function claim(address target, bytes32 hash, bytes memory signature) public whenNotPaused
    {
        require(target != address(0), "FinaSwap:claim Target address cannot be zero");
        require(hash[0] != 0, "FinaSwap:claim Signature hash is required");
        require(signature.length == 65, "FinaSwap:claim Invalid signature length. The signature must be exactly 65 bytes");

        //Recover the public key from the signature
        bytes memory publicKey = _ecdsa.recover(hash, signature);

        //The recovered compressed public key must be exact 33 bytes
        require(publicKey.length == 33, "FinaSwap:claim Invalid signature. The recovered public key must be exactly 33 bytes");

        //Hash the public key with the SHA256 hash function
        bytes32 publicKeySha256Hash = sha256(publicKey);

        //Hash the SHA256 hash of the public key with the RIPEMD-160 hash function
        bytes memory publicKeyRipe160Hash = abi.encodePacked(ripemd160(abi.encodePacked(publicKeySha256Hash)));

        //Add the network specific prefix to the hash
        bytes memory publicKeyPrefixedHash = bytes.concat(_prefix, publicKeyRipe160Hash);

        //Hash the prefixed public key hash twice with the SHA256 hash function
        bytes memory publicKeyDoubleSha256Hash = abi.encodePacked(sha256(abi.encodePacked(sha256(publicKeyPrefixedHash))));

        //Take the first 4 bytes from the double hashed public key
        bytes memory publicKeyPart = BytesUtils.slice(publicKeyDoubleSha256Hash, 0, 4);

        //Concatenate the prefixed public key hash and the first 4 bytes of the double hashed public key
        bytes memory publicKeyAddress = bytes.concat(publicKeyPrefixedHash, publicKeyPart);

        uint256 balance = _balances[publicKeyAddress];
        require(balance > 0, "FinaSwap:claim already swapped");

        _balances[publicKeyAddress] = 0;

        _token.mint(target, balance);

        if (_nativeValue > 0)
        {
            (bool sent,) = target.call{value: _nativeValue}("");
            require(sent, "Failed to send native currency");
        }
    }

    function addBalance(bytes memory source, uint256 balance) public onlyRole(ADMIN_ROLE)
    {
        _balances[source] = balance;
    }

    function removeBalance(bytes memory source) public onlyRole(ADMIN_ROLE)
    {
        _balances[source] = 0;
    }

    function setNativeValue(uint256 nativeValue) public onlyRole(ADMIN_ROLE)
    {
        _nativeValue = nativeValue;
    }

    function balanceOf(bytes memory source) public view returns (uint256)
    {
        return _balances[source];
    }

    function pause() public onlyRole(PAUSER_ROLE) {
        _pause();
    }

    function unpause() public onlyRole(PAUSER_ROLE) {
        _unpause();
    }    
}