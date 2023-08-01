const {execSync} = require('child_process');
const core = require('@actions/core');
const github = require('@actions/github');
const fs = require('fs');

async function main() {
    try {
        const githubToken = process.env.GITHUB_TOKEN;
        const githubRepo = process.env.GITHUB_REPOSITORY;
        const version = require('./package.json').version;
        const projectName = 'UnityAnimTool';
        const releaseTitle = `${projectName}_V${version}`;
        const fileName = `${projectName}_V${version}.zip`;
        const projectDirectory = '.';
        const excludePatterns = [
            './deploy_release.js',
        ];
        const excludeOptions = excludePatterns.map(
            (pattern) => `--exclude="${pattern}"`).join(' ');

        // Javascript에서 version 값을 사용하는 예시
        console.log('Version from package.json:', version);

        // 압축 파일 생성 (이 예시에서는 간단하게 zip으로 생성)
        execSync(`zip -r ${fileName} ${excludeOptions} ${projectDirectory}`);
        // console.log(`zip -r ${fileName} ${excludeOptions} ${projectDirectory}`)

        // GitHub 인스턴스 생성 및 레포지토리 가져오기
        const octokit = github.getOctokit(githubToken);
        const [owner, repo] = githubRepo.split('/');
        const repository = octokit.rest.repos.get({ owner, repo });

        // 릴리스 생성
        const release = await octokit.rest.repos.createRelease({
            owner,
            repository,
            tag_name: `V${version}`, // 태그를 "V버전" 형식으로 설정
            name: releaseTitle,
            body: 'Release Notes',
        });

        // 릴리스에 파일 첨부
        await octokit.rest.repos.uploadReleaseAsset({
            owner,
            repository,
            release_id: release.data.id,
            name: fileName,
            data: fs.readFileSync(fileName),
        });

        // 압축 파일 제거
        fs.unlinkSync(fileName);
    } catch (error) {
        console.error('Error occurred during deployment:', error);
        process.exit(1);
    }
}

main();
