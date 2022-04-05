// SPDX-License-Identifier: MIT
pragma solidity ^0.8.4;

library BytesUtils {
    function slice(bytes memory source, uint256 start, uint256 end) internal pure returns (bytes memory) {
        bytes memory a = new bytes(end - start);
        for(uint i=0;i<=end-start-1;i++){
            a[i] = source[i];
        }
        return a;
    }
}