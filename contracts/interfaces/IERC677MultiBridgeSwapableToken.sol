// SPDX-License-Identifier: MIT
pragma solidity ^0.8.4;

interface IERC677MultiBridgeSwapableToken {
    function swap(address _to, uint256 _amount) external;
}