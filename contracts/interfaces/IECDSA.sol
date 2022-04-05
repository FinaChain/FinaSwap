// SPDX-License-Identifier: MIT
pragma solidity ^0.8.4;
interface IECDSA {
    function recover(bytes32 hash, bytes memory signature) external view returns (bytes memory);
}